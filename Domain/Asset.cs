using System.Text.Json.Serialization;

namespace AssetTracking.Domain;

[JsonDerivedType(typeof(Computer),    typeDiscriminator: "Computer")]
[JsonDerivedType(typeof(MobilePhone), typeDiscriminator: "MobilePhone")]

// abstract = this class cannot be used directly; you must use Computer or MobilePhone instead.
public abstract class Asset
{
    // Every asset gets a unique number. The counter is shared across all instances.
    private static int _nextId = 1;

    // init setters let System.Text.Json populate these during deserialization.
    public int      Id           { get; init; }
    public string   Brand        { get; init; } = "";
    public string   Model        { get; init; } = "";
    public DateOnly PurchaseDate { get; init; }
    public decimal  PriceUsd     { get; init; }
    public Office   Office       { get; init; }

    [JsonIgnore]
    public abstract string AssetType { get; }

    [JsonIgnore]
    public DateOnly EndOfLife => PurchaseDate.AddYears(3);

    // Overridable in tests to inject a fixed reference date instead of the real clock.
    [JsonIgnore]
    protected virtual DateOnly Today => DateOnly.FromDateTime(DateTime.Today);

    // Positive = months remaining; negative = months past end-of-life. Day component is ignored.
    [JsonIgnore]
    public int MonthsRemaining
    {
        get
        {
            var today = Today;
            // Year difference converted to months, then add the leftover month difference.
            return (EndOfLife.Year - today.Year) * 12 + (EndOfLife.Month - today.Month);
        }
    }

    [JsonIgnore]
    public AssetStatus Status
    {
        get
        {
            var today = Today;
            if (today >= EndOfLife) return AssetStatus.Expired;  // past the three-year mark

            int months = (EndOfLife.Year - today.Year) * 12 + (EndOfLife.Month - today.Month);
            if (months < 3) return AssetStatus.Yellow;  // almost at end of life — act soon
            if (months < 6) return AssetStatus.Red;     // getting close — plan replacement
            return AssetStatus.None;                     // plenty of time left
        }
    }

    // Called by AssetRepository after loading from file so new assets don't collide with saved IDs.
    public static void ResetIdCounter(int nextId) => _nextId = nextId;

    // Used by System.Text.Json; Id is set via the init property, not _nextId.
    protected Asset() { }

    // protected = only Computer and MobilePhone can call this constructor, not outside code.
    protected Asset(string brand, string model, DateOnly purchaseDate, decimal priceUsd, Office office)
    {
        Id           = _nextId++;
        Brand        = brand;
        Model        = model;
        PurchaseDate = purchaseDate;
        PriceUsd     = priceUsd;
        Office       = office;
    }
}
