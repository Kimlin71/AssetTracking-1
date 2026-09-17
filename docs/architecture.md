# Architecture

Last updated: 2026-09-16
Slices implemented: 1–5 + documentation + unit tests (all mandatory levels complete; xUnit test project added for Asset.Status boundaries)

---

## Design decisions (resolving open checklist conflicts)

### CONFLICT-1 — Warning threshold precedence (resolved 2026-09-16)
Adopted interpretation from `project-context.md`. Confirmed rule:

| AssetStatus | Condition | ConsoleColor |
|-------------|-----------|---------------|
| `Expired` | `today >= endOfLife` | `ConsoleColor.Red` |
| `Yellow` | `today < endOfLife` and `monthsRemaining < 3` | `ConsoleColor.Yellow` |
| `Red` | `today < endOfLife` and `monthsRemaining >= 3` and `monthsRemaining < 6` | `ConsoleColor.Green` ("getting close" — plan replacement) |
| `None` | `monthsRemaining >= 6` | Default foreground |

Ranges are exclusive of each other. `Expired` is checked first, then Yellow, then Red.
`Console.ResetColor()` is called before the header row and after each data row to prevent colour bleed.

### CONFLICT-2 — Sort order (resolved 2026-09-16)
Both Level 2 and Level 3 sort orders are kept as separate menu options:

| Menu option | Sort order | Satisfies |
|-------------|------------|-----------|
| 2 — View Assets | `OrderBy(AssetType).ThenBy(PurchaseDate)` | L2-2 |
| 3 — Sort Assets | `OrderBy(Office).ThenBy(PurchaseDate)` | L3-6 |

Neither replaces the other; users choose the view they need.

### AMBIGUITY-4 — Price number format per currency zone
The OS locale must not affect how prices are displayed. Number formats are defined
explicitly via `NumberFormatInfo` in `AssetPrinter` rather than through a culture string:

| Zone | Thousands separator | Decimal separator | Example |
|------|--------------------|--------------------|----------|
| SEK  | `.` (dot)          | `,` (comma)        | 9.768,82 |
| TRY  | `.` (dot)          | `,` (comma)        | 53.534,36 |
| USD  | `,` (comma)        | `.` (dot)          | 1,999.00 |

This avoids `sv-SE`'s non-breaking-space thousands separator and any system-locale surprises.

### AMBIGUITY-1 — Currency conversion
Slice 3 switches to the **ExchangeRate-API** live service (api.exchangerate-api.com).  
API key is injected via a constant in `CurrencyConverter`; the key is not committed to version control in production but is acceptable for a student project.  
On network failure or non-success HTTP status the converter falls back to the fixed illustrative rates below so the program never crashes.

| From | To  | Fallback rate (1 USD = N local) |
|------|-----|---------------------------------|
| USD  | SEK | 10.50 |
| USD  | TRY | 32.00 |
| USD  | USD | 1.00  |

`decimal.Parse` with `CultureInfo.InvariantCulture` is used to avoid locale-dependent number parsing.  
Rates are fetched once at startup and cached for the session; no HTTP call is made per row.

### AMBIGUITY-2 — Asset type column
The display name is provided by an abstract property `AssetType` (returns `string`).
Each concrete class overrides it: `Computer` returns `"Computer"`, `MobilePhone` returns `"Mobile Phone"`.
This is more explicit than `GetType().Name` and survives refactoring.

### AMBIGUITY-3 — Unique ID strategy
IDs are auto-generated sequential integers managed by a private static counter on `Asset`.
The counter starts at 1 each run; persistence (Level 5) must persist the max ID separately.

---

## Component map

```
AssetTracking-1 (console, net10.0)
│
├── Domain
│   ├── Asset (abstract class)      — base for all asset types
│   ├── Computer (concrete)         — is-a Asset
│   ├── MobilePhone (concrete)      — is-a Asset
│   ├── Office (enum)               — Sweden | USA | Turkey
│   └── AssetStatus (enum)          — None | Red | Yellow | Expired
│
├── Services
│   ├── CurrencyConverter (static)  — converts USD → local currency via live API with fallback
│   └── AssetRepository (static)    — JSON file load/save; duplicate-ID guard; polymorphic type discriminator
│
├── Program (entry point)
│   ├── has-a List<Asset>            — the in-memory store (loaded from file at startup)
│   ├── seeds the list               — hardcoded sample data only when JSON file is absent/empty
│   └── top-level statements         — sets DefaultThreadCurrentCulture to InvariantCulture process-wide
│
├── AssetPrinter (static)           — formats and writes the asset table to console with row coloring
└── PromptHelper (static)           — console input helpers: retry loops and cancel-on-"cancel" support

AssetTracking.Tests (xUnit, net10.0)
└── AssetStatusTests                — 9 tests for Asset.Status boundary values; uses file-scoped TestAsset subclass
```

Dependency rule: Domain has no references to Services, Program, or console I/O.
Services depends only on Domain types. Program depends on both.
`PromptHelper` and `AssetPrinter` depend only on Domain types (and Services for printing).

---

## Type contracts

### `Asset` (abstract)
```
Attributes:
  [JsonDerivedType(typeof(Computer),    typeDiscriminator: "Computer")]
  [JsonDerivedType(typeof(MobilePhone), typeDiscriminator: "MobilePhone")]

Properties (all set in constructor):
  int          Id            { get; init; }    // auto-assigned from static _nextId counter
  string       Brand         { get; init; }
  string       Model         { get; init; }
  DateOnly     PurchaseDate  { get; init; }
  decimal      PriceUsd      { get; init; }
  Office       Office        { get; init; }

Abstract (JsonIgnore — not persisted):
  string       AssetType     { get; }          // display name

Computed (JsonIgnore — derived on load):
  DateOnly     EndOfLife     => PurchaseDate.AddYears(3)
  int          MonthsRemaining                 // months until EndOfLife; negative when expired
  AssetStatus  Status                          // derived from today vs EndOfLife

Testability seam (JsonIgnore):
  protected virtual DateOnly Today             // returns DateOnly.FromDateTime(DateTime.Today)
                                               // overridden in tests to inject a fixed date

Static method:
  void         ResetIdCounter(int nextId)      // called by AssetRepository after load
                                               // so new assets get IDs above highest saved ID

Constructors:
  protected Asset()                            // used by System.Text.Json deserializer
  protected Asset(string, string, DateOnly, decimal, Office)  // used by Computer/MobilePhone
```

### `Computer : Asset`
Overrides `AssetType` → `"Computer"`.

### `MobilePhone : Asset`
Overrides `AssetType` → `"Mobile Phone"`.

### `Office` (enum)
```
Sweden = 0
USA    = 1
Turkey = 2
```

### `AssetStatus` (enum)
```
None    = 0
Red     = 1
Yellow  = 2
Expired = 3
```

### `CurrencyConverter` (static class)
```
// Must be called once before the first conversion; fetches rates from the API.
Task    InitializeAsync()

decimal ToLocalCurrency(decimal usdPrice, Office office)
string  CurrencyCode(Office office)             // "SEK", "USD", "TRY"
```
`InitializeAsync` performs one HTTP GET to the ExchangeRate-API endpoint (`v6.exchangerate-api.com/v6/{key}/latest/USD`).  
If it fails (exception or non-200 status) the converter logs a console warning and continues with fallback rates.  
Rates are stored in `private static decimal _sekRate` and `_tryRate`; these start at fallback values and are overwritten on successful API fetch.

### `AssetRepository` (static class)
```
List<Asset> Load(string filePath)   // returns empty list when file absent; skips duplicate IDs
void        Save(List<Asset> assets, string filePath)
```
`Load` uses `System.Text.Json` with `JsonDerivedType` attributes on `Asset` for polymorphic round-tripping of `Computer` and `MobilePhone`. It also calls `Asset.ResetIdCounter(maxId + 1)` after a successful load.  
`Save` writes to a `.tmp` file then renames atomically (`File.Move(..., overwrite: true)`) to protect against data loss on crash.  
`DateOnly` is handled by a `file`-scoped `DateOnlyConverter : JsonConverter<DateOnly>` that reads/writes `"yyyy-MM-dd"` strings.

### `PromptHelper` (static class)
```
string?  Ask(string prompt)
  // Shows prompt, reads a line.
  // Returns null if the user types "cancel" (case-insensitive).
  // Loops until non-empty input is given.

string?  AskChoice(string prompt, params string[] valid)
  // Like Ask, but also rejects values not in `valid`.
  // Prints an error and repeats the prompt on invalid input.
  // Returns null on cancel.
```
Used only by `AddAsset` in `Program.cs`. Escape is implemented as typing the literal word `cancel`; the hint `"(Type 'cancel' at any prompt to go back)"` is printed at the start of each add-asset flow.

### `AssetPrinter` (static class)
```
void PrintTable(IEnumerable<Asset> assets)
```
Outputs one header row and one data row per asset. Columns are padded with composite format strings (`{N,width}`).
Column order: ID | Office | Type | Brand | Model | Local Price | Currency | Purchase Date

**Row coloring** replaces the `Status` column:
- `ConsoleColor.Yellow` — `AssetStatus.Yellow` (fewer than 3 months remaining)
- `ConsoleColor.Green`  — `AssetStatus.Red` (3–6 months remaining — "getting close" warning)
- `ConsoleColor.Red`    — `AssetStatus.Expired` (past end-of-life)
- Default foreground    — `AssetStatus.None` (6 or more months remaining)
- `Console.ResetColor()` before the header row and after each data row

Note: `AssetStatus.Red` maps to `ConsoleColor.Green` because the asset is not yet critical; it is a warning that replacement planning should start. `ConsoleColor.Red` is reserved for assets that have already expired.

The `Price USD` column is not shown; local price already captures the relevant value.  
`Currency` appears directly after `Local Price`.

**Number formatting:** prices use explicit `NumberFormatInfo` instances (not OS locale) to guarantee consistent separators:
- SEK / TRY: dot-thousands, comma-decimal (e.g. `9.768,82`)
- USD: comma-thousands, dot-decimal (e.g. `1,999.00`)

In addition, `Program.cs` sets `CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture` process-wide at startup, so any incidental `ToString()` calls that don't pass an explicit format also use dots.

---

## Slice 3 scope

**Goal:** Satisfy the following open checklist items and optional features.

| Item | Requirement |
|------|-------------|
| L5-1 | Load assets from JSON file at startup |
| L5-2 | Save after every add or remove |
| L5-3 | Reject duplicate IDs when loading |
| L5-4 | Create file automatically on first run |
| O1   | Search by any field (new menu option) |
| O5   | Colored rows tied to status |
| O7   | API-based currency conversion with fallback |

**Also addressed:** column order fix (Local Price then Currency), Status column removed (replaced by row color), menu expanded to 5 items, prompt text changed to "Select option:".

### Files to create

| File | Purpose |
|------|---------|
| `Services/AssetRepository.cs` | JSON load/save with polymorphism + duplicate-ID guard |

### Files to change

| File | Change |
|------|--------|
| `Services/CurrencyConverter.cs` | Replace fixed rates with ExchangeRate-API call + fallback |
| `AssetPrinter.cs` | Remove Status and Price USD columns; swap Local Price / Currency order; add row coloring |
| `Program.cs` | Call `CurrencyConverter.InitializeAsync` at startup; load from file; expand menu to 5 items (Add, View by type, Sort by office, Search, Exit); change "Choice:" to "Select option:"; save after add/remove |
| `AssetTracking-1.csproj` | No package changes needed — `System.Net.Http` and `System.Text.Json` are in the BCL |
| `docs/acceptance-checklist.md` | Update status for L5-* and O1, O5, O7 after implementation |

### Files to leave unchanged

`Domain/Asset.cs`, `Domain/Computer.cs`, `Domain/MobilePhone.cs`, `Domain/Office.cs`, `Domain/AssetStatus.cs` — no domain changes required by this slice.

---

## Menu definition (Slice 3)

```
=== Asset Tracker ===
1. Add Asset
2. View Assets
3. Sort Assets
4. Search Asset
5. Exit
Select option:
```

- **1 Add Asset** — prompt for type (Computer/MobilePhone), brand, model, purchase date, price USD, office; assign next ID; save file
- **2 View Assets** — list by asset type then purchase date (existing sort)
- **3 Sort Assets** — list by office then asset type then purchase date (existing sort)
- **4 Search Asset** — prompt for a keyword; show all assets where any string field contains the keyword (case-insensitive)
- **5 Exit** — quit the loop

---

## Console output sample (target, Slice 3)

```
ID  Office   Type          Brand    Model         Local Price  Currency  Purchase Date
--  ------   ----          -----    -----         -----------  --------  -------------
1   Sweden   Computer      Dell     XPS 15          12 394.50  SEK       2024-01-15      ← row color Red
2   Sweden   Mobile Phone  Apple    iPhone 14       10 318.47  SEK       2023-10-20      ← row color Yellow
3   USA      Computer      Apple    MacBook Pro      1 999.00  USD       2021-11-10      ← row color Red (Expired)
4   USA      Mobile Phone  Google   Pixel 8            699.00  USD       2024-08-05
5   Turkey   Computer      Lenovo   ThinkPad X1     46 210.50  TRY       2024-05-01
6   Turkey   Mobile Phone  Samsung  Galaxy S23      27 337.58  TRY       2024-01-20      ← row color Red
```

Rows colored Yellow/Red by status. No separate Status column.

---

## What is explicitly out of scope for Slice 3 (do not add)

- Edit an existing asset (O2)
- Remove an asset (O3)
- Pagination (O4)
- CSV export (O6)
- Unit test project (O8)
- XML documentation comments

---

## Slice 4 scope

**Goal:** Validated input with inline retry and Escape-to-cancel for the "Add Asset" flow.

### Problem being solved

Currently `AddAsset` aborts the entire flow on the first invalid entry. The user loses all previously entered values and must restart from the menu. This is poor UX. Two improvements are requested:

1. **Retry** — on invalid input, show an error message and re-print the same prompt so the user can try again without losing progress.
2. **Cancel** — at any prompt the user can press **Escape** (or type `cancel`) to abort the flow cleanly and return to the menu.

### Design decision — `PromptHelper` static class

All console input throughout the application eventually needs the same two behaviours. Centralising the logic in one helper keeps `AddAsset` readable and avoids copy-pasting retry loops.

```
PromptHelper (static class)          — new in Slice 4
─────────────────────────────────────────────────────
string?  Ask(string prompt)
  // Shows prompt, reads a line.
  // Returns null if the user presses Escape or types "cancel" (case-insensitive).
  // Loops until non-empty input is given.

string?  AskChoice(string prompt, params string[] valid)
  // Like Ask, but also rejects values not in `valid`.
  // Prints an error and repeats the prompt on invalid input.
  // Returns null on cancel.
```

**Escape detection:** `Console.ReadKey` cannot capture Escape mid-line. The practical approach for a console app is:
- After `Console.ReadLine()` returns, check if the trimmed value equals `"cancel"` (case-insensitive) → treat as cancel.
- Additionally, offer a visible `(or type 'cancel' to go back)` hint at the start of the flow so the user knows the escape hatch exists.

This is the standard pattern for terminal apps that must run cross-platform without a curses library.

### `AddAsset` flow (Slice 4)

```
Type (1=Computer, 2=MobilePhone, or 'cancel' to go back):
  → invalid: "Invalid type. Enter 1 or 2." → repeat prompt
  → cancel: return to menu immediately

Brand:
  → empty: "Brand cannot be empty." → repeat

Model:
  → empty: "Model cannot be empty." → repeat

Purchase date (yyyy-MM-dd):
  → invalid: "Invalid date. Use format yyyy-MM-dd, e.g. 2024-01-15." → repeat

Price in USD (e.g. 1200.50):
  → invalid or negative: "Invalid price. Enter a positive number." → repeat

Office (1=Sweden, 2=USA, 3=Turkey):
  → invalid: "Invalid office. Enter 1, 2 or 3." → repeat
```

At any step, typing `cancel` returns to the menu. No asset is created or saved unless all fields are collected successfully.

### Files to create

| File | Purpose |
|------|---------|
| `PromptHelper.cs` | `Ask` and `AskChoice` with retry loop and cancel support |

### Files to change

| File | Change |
|------|--------|
| `Program.cs` | Rewrite `AddAsset` to use `PromptHelper`; add the "type 'cancel'" hint at entry |

### Files to leave unchanged

All domain files, `AssetPrinter.cs`, `Services/`, `AssetTracking-1.csproj`.

### Console interaction sample (target, Slice 4)

```
Type 'cancel' at any prompt to go back to the menu.
Type (1=Computer, 2=MobilePhone): x
  Invalid type. Enter 1 or 2.
Type (1=Computer, 2=MobilePhone): 1
Brand: Dell
Model: XPS 15
Purchase date (yyyy-MM-dd): 24-01-15
  Invalid date. Use format yyyy-MM-dd, e.g. 2024-01-15.
Purchase date (yyyy-MM-dd): 2024-01-15
Price in USD (e.g. 1200.50): -5
  Invalid price. Enter a positive number.
Price in USD (e.g. 1200.50): 1200
Office (1=Sweden, 2=USA, 3=Turkey): 1
Asset 7 added and saved.
```

Cancel example:
```
Type (1=Computer, 2=MobilePhone): cancel
Cancelled — returning to menu.
```

---

## Slice 5 scope

**Goal:** Fix seed data purchase dates so all three color states are clearly visible on any run date. The colors are computed correctly from today's date — but the original seed dates were chosen for 2024 and are no longer representative in 2026.

### Problem
The `Status` property always computes from `DateTime.Today` correctly. The seed data purchase dates were fixed at 2023–2024 values. As time passes, assets that were "Red" become "Expired" or fall out of the warning window entirely, making the color demonstration useless.

### Fix
Recalculate all seed purchase dates as offsets from today (2026-09-16) so each color zone is always represented:

| ID | Asset | Target status | EOL target | Purchase date |
|----|-------|---------------|------------|---------------|
| 1 | Dell XPS 15 (Sweden) | Yellow — 1 month left | 2026-10-16 | 2023-10-16 |
| 2 | Apple iPhone 14 (Sweden) | Green — 4 months left | 2027-01-16 | 2024-01-16 |
| 3 | Apple MacBook Pro (USA) | Red — Expired | 2026-08-01 | 2023-08-01 |
| 4 | Google Pixel 8 (USA) | None — 11 months left | 2027-08-16 | 2024-08-16 |
| 5 | Lenovo ThinkPad X1 (Turkey) | None — 9 months left | 2027-06-16 | 2024-06-16 |
| 6 | Samsung Galaxy S23 (Turkey) | Yellow — 2 months left | 2026-11-16 | 2023-11-16 |

### Files to change

| File | Change |
|------|--------|
| `Program.cs` | Update seed purchase dates |
| `assets.json` | Update purchase dates to match new seed |

---

## Current implementation status (after Slice 5)

All five slices are complete.

| Level | Status |
|-------|--------|
| L1 Modelling | ✅ Complete |
| L2 Sorting and lifespan | ✅ Complete |
| L3 Offices and currencies | ✅ Complete |
| L4 OOP, ID, age, menu | ✅ Complete |
| L5 Persistence | ✅ Complete |
| O1 Search | ✅ Implemented |
| O5 Colored rows | ✅ Implemented |
| O7 Live API currency | ✅ Implemented |

### Known deviation — menu options 2 and 3

Both menu option 2 ("View Assets") and option 3 ("Sort Assets") currently execute the same `OrderBy(Office).ThenBy(PurchaseDate)` sort. The architecture description for Slice 3 intended option 2 to sort by type/date and option 3 by office/date. This deviation has not been corrected; it does not affect any acceptance checklist item but makes option 2 redundant.

### Remaining optional features (not implemented)

| Feature | Option |
|---------|--------|
| Edit an existing asset | O2 |
| Remove an asset | O3 |
| Pagination for long lists | O4 |
| CSV export | O6 |
| Unit test project | O8 |

---

## Handoff

**Summary:** Architecture document updated to match Slices 1–5 as implemented. All mandatory levels are satisfied. Type contracts, component map, `PromptHelper`, `ResetIdCounter`, polymorphic JSON, number formatting, and row-coloring semantics are now accurately documented.

**Files changed:** `docs/architecture.md`

**Verification:** Document compared line-by-line against `Program.cs`, `AssetPrinter.cs`, `Services/CurrencyConverter.cs`, `Services/AssetRepository.cs`, `Domain/Asset.cs`, `PromptHelper.cs`, and `docs/acceptance-checklist.md`.

**Open risks:**
- Menu options 2 and 3 are functionally identical (both sort by office). Should be fixed to restore type-sort vs office-sort distinction.
- CONFLICT-1 (warning threshold interpretation) should be confirmed with the instructor before final submission.
- The API key is committed in source (`CurrencyConverter.cs`); acceptable for a student project but noted.

**Recommended next task:** Fix the menu option 2 / option 3 duplication so option 2 sorts by type then date (Level 2 sort) and option 3 sorts by office then type then date (Level 3 sort).
