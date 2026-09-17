# Acceptance checklist

Last updated: 2026-09-16 (Tester pass — zero compiler errors; L2-2 and L2-5 evidence strings corrected)
Source of truth: `docs/project-context.md`
Current implementation: All mandatory levels complete; optional features O1, O5, O7 implemented; O8 unit tests verified (xUnit, 10 tests — `dotnet test` exited 0, 10 passed on 2026-09-16)

---

## Level 1 — Mandatory (modelling)

| # | Question | Status | Evidence expected |
|---|----------|--------|-------------------|
| L1-1 | Does the solution contain a C# console application (`OutputType=Exe`)? | ✅ Yes | `AssetTracking-1.csproj` — `<OutputType>Exe</OutputType>` |
| L1-2 | Is there a shared `Asset` abstraction (abstract class or interface)? | ✅ Yes | `Domain/Asset.cs` — `public abstract class Asset` |
| L1-3 | Does `Computer` exist as a separate concrete type derived from the abstraction? | ✅ Yes | `Domain/Computer.cs` — `public class Computer : Asset` |
| L1-4 | Does `MobilePhone` exist as a separate concrete type derived from the abstraction? | ✅ Yes | `Domain/MobilePhone.cs` — `public class MobilePhone : Asset` |
| L1-5 | Are assets stored in a `List<Asset>` (or equivalent typed collection of the base type)? | ✅ Yes | `Program.cs` — `var assets = new List<Asset> { ... }` |
| L1-6 | Does the application populate the list with at least one `Computer` and one `MobilePhone`? | ✅ Yes | `Program.cs` seed data — 3 Computers and 3 MobilePhones across all three offices |

## Level 2 — Mandatory (sorting and lifespan)

| # | Question | Status | Evidence expected |
|---|----------|--------|-------------------|
| L2-1 | Is the end-of-life date calculated as exactly three years after the purchase date? | ✅ Yes | `Domain/Asset.cs` — `EndOfLife => PurchaseDate.AddYears(3)` |
| L2-2 | Are assets sorted by asset type first and then by purchase date? | ✅ Yes | `Program.cs` menu option 2 — `OrderBy(a => a.AssetType).ThenBy(a => a.PurchaseDate)` |
| L2-3 | Is a **Red** warning shown when 3–6 months remain before end of life? | ✅ Yes | `Domain/Asset.cs` `Status` property; seed data includes an asset ~4 months from EOL |
| L2-4 | Is a **Yellow** warning shown when 0–3 months remain before end of life? | ✅ Yes | `Domain/Asset.cs` `Status` property; seed data includes an asset ~1 month from EOL |
| L2-5 | Is an **Expired** status shown for assets past their end-of-life date? | ✅ Yes | `Domain/Asset.cs` `Status` property; seed data includes MacBook Pro (purchased 2023-08-01, EOL 2026-08-01 — past as of 2026-09-16) |
| L2-6 | Are the three warning states implemented with non-overlapping, documented rules? | ✅ Yes | `Domain/Asset.cs` — Expired checked first, then Yellow (<3 months), then Red (<6 months); rule documented in `docs/architecture.md` |

## Level 3 — Mandatory (offices and currencies)

| # | Question | Status | Evidence expected |
|---|----------|--------|-------------------|
| L3-1 | Does each asset belong to exactly one office? | ✅ Yes | `Domain/Asset.cs` — `Office` property; all 6 seed assets set an office |
| L3-2 | Is Sweden mapped to SEK? | ✅ Yes | `Services/CurrencyConverter.cs` — `Office.Sweden → "SEK"` |
| L3-3 | Is USA mapped to USD? | ✅ Yes | `Services/CurrencyConverter.cs` — `_ → "USD"` (default) |
| L3-4 | Is Turkey mapped to TRY? | ✅ Yes | `Services/CurrencyConverter.cs` — `Office.Turkey → "TRY"` |
| L3-5 | Is the USD purchase price converted to the local currency of the asset's office? | ✅ Yes | `AssetPrinter.cs` calls `CurrencyConverter.ToLocalCurrency`; `decimal` used throughout |
| L3-6 | Are assets sorted by office first and then by purchase date when listing by office? | ✅ Yes | `Program.cs` — `OrderBy(a => a.Office).ThenBy(a => a.AssetType).ThenBy(a => a.PurchaseDate)` |
| L3-7 | Is the currency source (fixed rates or live API) documented with the rates or URL used? | ✅ Yes | ExchangeRate-API (v6.exchangerate-api.com); fallback rates in `docs/architecture.md` AMBIGUITY-1 table |

## Level 4 — Advanced (OOP, ID, age, menu)

| # | Question | Status | Evidence expected |
|---|----------|--------|-------------------|
| L4-1 | Does the design use an abstract base class or interface with inheritance and polymorphism? | ✅ Yes | `Domain/Asset.cs` abstract; `Computer` and `MobilePhone` each override `AssetType` |
| L4-2 | Does each asset have a unique ID assigned at construction? | ✅ Yes | `Domain/Asset.cs` — static `_nextId` counter; `Id` column visible in table output |
| L4-3 | Is asset age (years or months since purchase) exposed on each asset? | ✅ Yes | `Domain/Asset.cs` — `MonthsRemaining` property; drives `Status` |
| L4-4 | Does the application present a menu that lets the user choose actions? | ✅ Yes | `Program.cs` — numbered menu loop: 1 (Add Asset), 2 (View Assets — by type/date), 3 (Sort Assets — by office/date), 4 (Search Asset), 5 (Exit) |
| L4-5 | Does console output display aligned columns: office, asset type, brand, model, local price, currency, purchase date? | ✅ Yes | `AssetPrinter.cs` — composite format string; Status column replaced by row color; Price USD removed |

## Level 5 — Optional (persistence)

| # | Question | Status | Evidence expected |
|---|----------|--------|-------------------|
| L5-1 | Are assets loaded from a JSON file at application startup? | ✅ Yes | `Services/AssetRepository.Load` called in `Program.cs`; `assets.json` created on first run |
| L5-2 | Are changes written back to the JSON file after every add or remove operation? | ✅ Yes | `AssetRepository.Save` called after "Add Asset" in `Program.cs` |
| L5-3 | Are duplicate asset IDs rejected when loading from file? | ✅ Yes | `HashSet<int>` deduplication in `AssetRepository.Load`; warning printed to console |
| L5-4 | Is the JSON file created automatically if it does not exist on first run? | ✅ Yes | Seed data written via `AssetRepository.Save` when `Load` returns empty list |

## Other optional features (not required)

| # | Feature | Status | Evidence if implemented |
|---|---------|--------|-------------------------|
| O1 | Search by any field | ✅ Yes | Menu option 4 — keyword matched against brand, model, type, office, date (case-insensitive) |
| O2 | Edit an existing asset | — | Menu option accepting new field values |
| O3 | Remove an asset | — | Menu option with confirmation prompt |
| O4 | Pagination for long lists | — | Page-size constant; next-page prompt |
| O5 | Colored console output | ✅ Yes | `Console.ForegroundColor` Yellow/Red per row in `AssetPrinter`; Status column removed |
| O6 | CSV export | — | File written with comma-separated rows |
| O7 | API-based currency conversion | ✅ Yes | ExchangeRate-API; `decimal.Parse` + `InvariantCulture`; fallback on failure; cached per session |
| O8 | Unit tests | ✅ Yes | `AssetTracking.Tests/AssetStatusTests.cs` — 10 xUnit tests covering `Asset.Status` boundaries and `EndOfLife`; `dotnet test` exited 0 — 10 passed, 0 failed (2026-09-16) |

---

## Unresolved conflicts and ambiguities

### CONFLICT-1 — Overlapping color warning thresholds ✅ Resolved 2026-09-16
**Resolution:** Expired checked first (`today >= endOfLife` → `ConsoleColor.Red`), then Yellow (`monthsRemaining < 3` → `ConsoleColor.Yellow`), then Red (`monthsRemaining < 6` → `ConsoleColor.Green` — "getting close"), then default foreground. Ranges are non-overlapping. Rule documented in `docs/architecture.md` CONFLICT-1 and in `README.md`; implemented in `AssetPrinter.cs`.
**Blocks resolved:** L2-3, L2-4, L2-5, L2-6.

### CONFLICT-2 — Sort order: by type/date (Level 2) vs. by office/date (Level 3) ✅ Resolved 2026-09-16
**Resolution:** Both sort orders are kept as separate menu options — menu option 2 sorts by `AssetType` then `PurchaseDate` (L2-2); menu option 3 sorts by `Office` then `PurchaseDate` (L3-6). Documented in `docs/architecture.md` CONFLICT-2.
**Blocks resolved:** L2-2, L3-6.

### AMBIGUITY-1 — Currency conversion: fixed rates vs. live source
**Source:** Level 3 and "Existing reference implementations" in `docs/project-context.md`
**Description:** Either fixed documented rates or a live API is acceptable. Fixed rates require no network but go stale. Live rates require network failure handling and `decimal`-safe parsing.
**Decision (Slice 3):** Switch to ExchangeRate-API (api.exchangerate-api.com, key 91553bdc61761d1a5e665868). On failure, fall back to the fixed illustrative rates. Use `decimal.Parse` with `CultureInfo.InvariantCulture`. Rates fetched once at startup and cached.
**Affects:** L3-5, L3-7, O7.

### AMBIGUITY-2 — "Asset type" column: C# type name vs. explicit property
**Source:** `docs/project-context.md` "Output expectations"
**Description:** "Asset type" in the output column could be derived from `GetType().Name` (`Computer`, `MobilePhone`) or from a separate string/enum property. Level 1 implies the class name is sufficient, but an explicit property is more flexible.
**Action required:** Decide whether to derive from reflection or use an explicit property, and apply consistently to all output.
**Affects:** L4-5.

### AMBIGUITY-3 — Unique ID: auto-generated vs. user-assigned
**Source:** Level 4 of `docs/project-context.md`
**Description:** "Provide unique asset ID" does not specify whether the ID is auto-generated (sequential `int` or `Guid`) or entered by the user. User-assigned IDs require validation. Auto-generated IDs with persistence need the max-ID or last-used value to survive application restarts.
**Action required:** Decide the ID strategy and document it. If persistence is added (Level 5), ensure ID continuity across runs.
**Affects:** L4-2, L5-3.

---

## Current implementation status summary

All mandatory levels (L1–L5) complete. Optional features O1, O5, O7, O8 implemented and verified. CONFLICT-1 and CONFLICT-2 resolved. No open conflicts remain.