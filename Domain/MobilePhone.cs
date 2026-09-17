namespace AssetTracking.Domain;

// MobilePhone is a specific kind of Asset. It inherits all properties from Asset.
public class MobilePhone : Asset
{
    // override = replace the abstract definition in Asset with this concrete answer.
    public override string AssetType => "Mobile Phone";

    public MobilePhone() { }  // required by System.Text.Json for deserialization

    public MobilePhone(string brand, string model, DateOnly purchaseDate, decimal priceUsd, Office office)
        : base(brand, model, purchaseDate, priceUsd, office) { }
}
