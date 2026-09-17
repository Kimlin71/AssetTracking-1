namespace AssetTracking.Domain;

// An enum is a fixed list of named choices. Office tells us which country office owns an asset.
public enum Office
{
    Sweden = 0,  // uses SEK (Swedish krona)
    USA    = 1,  // uses USD (US dollar) — no conversion needed
    Turkey = 2,  // uses TRY (Turkish lira)
}
