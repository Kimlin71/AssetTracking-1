namespace AssetTracking.Domain;

// Computer is a specific kind of Asset. It inherits all properties from Asset.
public class Computer : Asset
{
    // override = replace the abstract definition in Asset with this concrete answer.
    public override string AssetType => "Computer";

    public Computer() { }  // required by System.Text.Json for deserialization

    public Computer(string brand, string model, DateOnly purchaseDate, decimal priceUsd, Office office)
        : base(brand, model, purchaseDate, priceUsd, office) { }
}
