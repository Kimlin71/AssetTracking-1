namespace AssetTracking.Domain;

// The four possible end-of-life warning states an asset can be in.
public enum AssetStatus
{
    None    = 0,  // more than 6 months left — no warning needed
    Red     = 1,  // 3–6 months left — start planning a replacement
    Yellow  = 2,  // 0–3 months left — replacement is urgent
    Expired = 3,  // past the three-year end-of-life date
}
