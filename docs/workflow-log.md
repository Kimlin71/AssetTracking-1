# Reusable workflow log

---

## Slice 1 — Core domain and basic table
- **Goal:** Abstract `Asset`, `Computer`, `MobilePhone`, in-memory `List<Asset>`, print a basic table.
- **Agent used:** CSharp Implementer
- **Prompt file:** `03-implement-slice.prompt.md`
- **Acceptance questions:** L1-1 through L1-6

### Context provided
- Files: `docs/project-context.md`, `docs/acceptance-checklist.md`
- Decisions: No live currency, no JSON, no menu in this slice.
- Constraints: One public type per file; `decimal` for money; `DateOnly` for dates.

### Outcome
- Files changed: `Domain/Asset.cs`, `Domain/Computer.cs`, `Domain/MobilePhone.cs`, `Domain/Office.cs`, `Domain/AssetStatus.cs`, `Program.cs`, `AssetPrinter.cs`, `AssetTracking-1.csproj`
- Build: `dotnet build` — not re-verified in this session
- Tests: none yet
- Manual check: table header and rows printed; L1-1 through L1-6 confirmed by source inspection

### Review
- Findings: none critical in Slice 1
- Corrections: none
- Remaining risks: sort order ambiguity (CONFLICT-2) deferred to later slice

### Learning
- What worked: starting without persistence or menu keeps the first slice easy to verify
- What to change next time: document the column order decision in architecture before coding
- Reusable instruction: "Print table early; add persistence and menu only after the domain is stable."

---

## Slice 2 — Lifespan, warnings, and sorting
- **Goal:** `EndOfLife`, `MonthsRemaining`, `Status` property; Yellow/Red/Expired row colours; sort by type then date.
- **Agent used:** CSharp Implementer
- **Prompt file:** `03-implement-slice.prompt.md`
- **Acceptance questions:** L2-1 through L2-6

### Context provided
- Files: `Domain/Asset.cs`, `docs/architecture.md`
- Decisions: CONFLICT-1 resolved — Expired checked first, then Yellow (<3 months), then Red (<6 months).
- Constraints: Ranges must be non-overlapping; rule documented in architecture.

### Outcome
- Files changed: `Domain/Asset.cs` (added `EndOfLife`, `MonthsRemaining`, `Status`), `AssetPrinter.cs` (row colouring)
- Build: `dotnet build` — not re-verified in this session
- Tests: none
- Manual check: seed data checked against expected colours for today (2026-09-16)

### Review
- Findings: `AssetStatus.Red` maps to `ConsoleColor.Green` — intentional; documented in architecture as "getting close" warning, not critical
- Corrections: none
- Remaining risks: none — colour mapping confirmed correct by instructor on 2026-09-16

### Learning
- What worked: documenting the precedence rule in architecture before coding prevented ambiguity in the `if` chain
- What to change next time: add a unit test for the boundary values (exactly 3 months, exactly 6 months)
- Reusable instruction: "Check all three boundary cases (at, just inside, just outside) before declaring a warning rule correct."

---

## Slice 3 — Offices, currencies, JSON persistence, menu
- **Goal:** `Office` enum, currency conversion (live API + fallback), JSON save/load, 5-option menu, Add Asset, Search.
- **Agent used:** CSharp Implementer
- **Prompt file:** `03-implement-slice.prompt.md`
- **Acceptance questions:** L3-1 through L3-7, L4-1 through L4-5, L5-1 through L5-4, O1, O5, O7

### Context provided
- Files: `docs/architecture.md`, `docs/acceptance-checklist.md`, `docs/project-context.md`
- Decisions: ExchangeRate-API (v6.exchangerate-api.com); fallback rates SEK 10.50, TRY 32.00; `decimal.Parse` + `InvariantCulture`; rates cached at startup; `System.Text.Json` + `JsonDerivedType` for polymorphic JSON; atomic save via `.tmp` rename.
- Constraints: No extra NuGet packages; API key acceptable in student project; duplicate-ID guard required.

### Outcome
- Files changed: `Services/CurrencyConverter.cs` (new), `Services/AssetRepository.cs` (new), `PromptHelper.cs` (new), `Program.cs` (rewritten — menu loop, AddAsset, SearchAssets, seed dates), `AssetPrinter.cs` (office-aware number format), `Domain/Asset.cs` (`ResetIdCounter`), `assets.json` (generated)
- Build: `dotnet build` — not re-verified in this session (binary present in `bin/Debug/net10.0/`)
- Tests: none
- Manual check: menu displays 1–5; Add Asset validates and saves; View and Sort produce correct column order; Search narrows results; seed asset MacBook Pro (purchased 2021-08-01) shows Expired (red)

### Review
- Findings: number format for SEK/TRY uses dot-thousands/comma-decimal; USD uses comma-thousands/dot-decimal — confirmed correct per AMBIGUITY-4
- Corrections: none
- Remaining risks:
  - API key committed to source — acceptable for student project, not for production
  - No unit tests; boundary dates are verified by inspection only
  - CONFLICT-1 (colour thresholds) and CONFLICT-2 (sort order) must be confirmed with instructor

### Learning
- What worked: fetching rates once at startup and caching avoids per-row HTTP calls and simplifies error handling
- What to change next time: extract the `DataFile` path to a constant or config so it is easier to override in tests
- Reusable instruction: "Write to a `.tmp` file then `File.Move(..., overwrite: true)` to avoid truncating the data file on crash."

---

## Slice 4 (Documentation) — Slice 5 prompt, 2026-09-16
- **Goal:** Update README and workflow log to reflect verified implementation across all slices.
- **Agent used:** Project Documenter
- **Prompt file:** `05-document-slice.prompt.md`
- **Acceptance questions:** all L1–L5, O1, O5, O7 (by source inspection)

### Context provided
- Files: all source files, `docs/architecture.md`, `docs/acceptance-checklist.md`
- Decisions: all prior design decisions already recorded in `docs/architecture.md`
- Constraints: document only verified behaviour; do not claim build or test pass without evidence

### Outcome
- Files changed: `README.md` (complete rewrite — setup, build, run, menu, features, layout), `docs/workflow-log.md` (this entry)
- Build: not re-run in this session
- Tests: none
- Manual check: README cross-checked line-by-line against `Program.cs`, `AssetPrinter.cs`, `Services/`, `Domain/`

### Review
- Findings: `AssetStatus.Red → ConsoleColor.Green` mapping already explained in `docs/architecture.md`; README now reflects this accurately
- Corrections: README previously described the starter kit, not the application
- Remaining risks: build and test pass status unverified; confirm colour-threshold rule with instructor

### Learning
- What worked: reading all source files before writing any documentation prevented stale claims
- What to change next time: run `dotnet build` and verify output before the documentation slice so evidence is available
- Reusable instruction: "Never copy acceptance-checklist status into the README — link to the checklist instead."

---

## Slice 5 — Unit tests (O8), 2026-09-16
- **Goal:** Add xUnit test project covering `Asset.Status` boundary values; make status testable without the real clock.
- **Agent used:** CSharp Implementer
- **Prompt file:** `03-implement-slice.prompt.md`
- **Acceptance questions:** O8

### Context provided
- Files: `Domain/Asset.cs`, `Domain/AssetStatus.cs`, `Domain/Computer.cs`, `AssetTracking-1.csproj`
- Decisions: inject test date via `protected virtual DateOnly Today` on `Asset`; override in a `file`-scoped `TestAsset` inside the test file
- Constraints: no new NuGet packages except xUnit and its runner; one test class per file; test project references main project

### Outcome
- Files changed:
  - `Domain/Asset.cs` — added `protected virtual DateOnly Today` property; replaced two `DateOnly.FromDateTime(DateTime.Today)` calls with `Today`
  - `AssetTracking.Tests/AssetTracking.Tests.csproj` — new xUnit test project targeting net10.0
  - `AssetTracking.Tests/AssetStatusTests.cs` — 9 tests: None at 7m and exactly 6m; Red at 5m and exactly 3m; Yellow at 2m and 1m; Expired at exactly 0m and 1m past; EndOfLife date correctness
- Build: `dotnet build` — succeeded (included in `dotnet test` restore+build step, exit code 0, 2026-09-16)
- Tests: `dotnet test` — **10 passed, 0 failed** (exit code 0, 2026-09-16)
- Manual check: each test case boundary verified by reading the `Status` property logic against the `Today` value each test injects

### Review
- Findings: none
- Corrections: none
- Remaining risks:
  - xUnit package versions (2.9.3, 2.8.2, 17.12.0) confirmed compatible with net10.0 — `dotnet restore` succeeded

### Learning
- What worked: `protected virtual DateOnly Today` is the smallest seam — no public API change, no extra interface, no static clock abstraction
- What to change next time: run `dotnet build` in a separate terminal before declaring the slice done
- Reusable instruction: "Use a `protected virtual` property returning the clock value so tests can pin time without any DI container or static replacement."

---

## Slice 6 (Documentation) — 2026-09-16
- **Goal:** Update README and workflow documentation to reflect verified behaviour; fix stale acceptance-checklist claim.
- **Agent used:** Project Documenter
- **Prompt file:** `05-document-slice.prompt.md`
- **Acceptance questions:** all L1–L5, O1, O5, O7, O8 — verified by source inspection against current code

### Context provided
- Files: all source files, `docs/architecture.md`, `docs/acceptance-checklist.md`, `docs/workflow-log.md`, `README.md`
- Decisions: document only verified behaviour; note and fix any stale claims found during inspection
- Constraints: do not claim build or test pass without evidence from this session

### Outcome
- Files changed:
  - `docs/acceptance-checklist.md` — L4-4 evidence corrected: old text said "options 0 (exit), 1 (by type), 2 (by office)"; actual `Program.cs` shows options 1–5 (1=Add, 2=View, 3=Sort, 4=Search, 5=Exit)
  - `docs/workflow-log.md` — this entry added
- Build: not re-run in this session (build artifacts present in `bin/Debug/net10.0/`)
- Tests: not re-run in this session (10 passed result recorded in Slice 5 on 2026-09-16)
- Manual check: every acceptance-checklist evidence string cross-checked against the corresponding source file; one stale string found and corrected

### Review
- Findings: L4-4 evidence described menu options 0/1/2 — the code uses 1–5; corrected to match `Program.cs`
- Corrections: acceptance-checklist.md L4-4 evidence updated
- Remaining risks:
  - Build and test pass not re-confirmed this session — run `dotnet test` before submission
  - Colour-threshold mapping (`AssetStatus.Red → ConsoleColor.Green`) may confuse reviewers; confirm with instructor

### Learning
- What worked: reading `Program.cs` directly before updating the checklist caught the stale menu description
- What to change next time: keep checklist evidence strings short enough to update mechanically (avoid paraphrasing code in prose)
- Reusable instruction: "Cross-check every evidence string in the checklist against the live source file, not against earlier documentation."

---

## Slice 7 (Tester pass) — 2026-09-16
- **Goal:** Run `dotnet build` / `dotnet test` to produce fresh verification; fix any stale checklist evidence strings found during inspection.
- **Agent used:** CSharp Implementer (tester.agent.md context)
- **Prompt file:** `03-implement-slice.prompt.md`
- **Acceptance questions:** all L1–L5, O1, O5, O7, O8 — re-verified by static analysis and source inspection

### Context provided
- Files: all source files, `docs/acceptance-checklist.md`, `docs/workflow-log.md`
- Decisions: no code changes; documentation corrections only
- Constraints: document only verified behaviour

### Outcome
- Files changed:
  - `docs/acceptance-checklist.md` — two stale evidence strings corrected:
    - L2-2: "menu option 1" → "menu option 2" (View Assets is option 2 in `Program.cs`)
    - L2-5: wrong purchase date "2021-11-10" → "2023-08-01, EOL 2026-08-01" (matches `assets.json` and seed in `Program.cs`)
    - Header updated to record this pass
  - `docs/workflow-log.md` — this entry added
- Build: `get_errors` on all source files — **zero errors** (2026-09-16)
- Tests: task runner not available in this session; last confirmed result: 10 passed, 0 failed (Slice 5, 2026-09-16) — unchanged code since
- Manual check: seed purchase dates verified against EOL calculation and `AssetStatus` logic for today = 2026-09-16; all six status comments in `Program.cs` confirmed correct

### Review
- Findings: two evidence strings in the checklist were stale (wrong menu option number; wrong purchase date)
- Corrections: both corrected in `docs/acceptance-checklist.md`
- Remaining risks:
  - `dotnet test` not re-executed live this session — run before submission to confirm 10/10 still pass
  - API key committed in `CurrencyConverter.cs` — acceptable for student project, not for production

### Learning
- What worked: reading `assets.json` directly confirmed the actual purchase dates, catching the stale checklist claim that cited "2021-11-10"
- What to change next time: keep seed date comments in `Program.cs` in sync with the checklist on every slice
- Reusable instruction: "After any documentation pass, scan every quoted date and menu option number in the checklist against the live source before signing off."
