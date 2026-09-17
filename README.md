# Asset Tracking — C# Console Application

A C# (.NET 10) console application that tracks company assets by office, type, purchase date, price, and currency. Built as an educational project covering OOP, polymorphism, JSON persistence, live currency conversion, and console UI.

## Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Internet access for live exchange-rate lookup (optional — fallback rates are used on failure)

## Build
```bash
dotnet build
```

## Run
```bash
dotnet run
```

The data file `assets.json` is created automatically next to the executable on first run. Six seed assets are added when no saved data exists.

## Menu options
| Option | Action |
|--------|--------|
| 1 | Add Asset (validated prompts; type `cancel` to abort) |
| 2 | View Assets — sorted by type then purchase date |
| 3 | Sort Assets — sorted by office then purchase date |
| 4 | Search Asset — keyword matched against brand, model, type, office, date |
| 5 | Exit |

## Console output columns
`ID | Office | Type | Brand | Model | Local Price | Currency | Purchase Date`

Row colour replaces a status column:
- **Yellow** — fewer than 3 months before end of life
- **Green** — 3–6 months before end of life (plan replacement)
- **Red** — past end of life (expired)
- Default — 6 or more months remaining

## Currency
Rates are fetched from [ExchangeRate-API](https://www.exchangerate-api.com) once at startup. If the API is unreachable the program continues with documented fallback rates (USD→SEK 10.50, USD→TRY 32.00) and prints a warning.

## Features implemented
| Level | Feature | Status |
|-------|---------|--------|
| 1 | `Asset` abstract class; `Computer` and `MobilePhone` subclasses; `List<Asset>` | ✅ |
| 2 | 3-year end-of-life; sort by type/date; Yellow/Red/Expired warnings | ✅ |
| 3 | Office enum (Sweden/USA/Turkey); SEK/USD/TRY currency conversion; sort by office/date | ✅ |
| 4 | OOP polymorphism; unique IDs; `MonthsRemaining`; 5-option menu | ✅ |
| 5 | JSON persistence (`assets.json`); atomic save; duplicate-ID guard | ✅ |
| O1 | Keyword search | ✅ |
| O5 | Coloured console rows | ✅ |
| O7 | Live API currency with fallback | ✅ |

## Project layout
```
Domain/          — Asset (abstract), Computer, MobilePhone, Office (enum), AssetStatus (enum)
Services/        — CurrencyConverter (static), AssetRepository (static)
AssetPrinter.cs  — formats and colours the console table
PromptHelper.cs  — validated console-input helpers
Program.cs       — entry point, menu loop, seed data
assets.json      — persisted asset list (auto-created)
docs/            — architecture, acceptance checklist, workflow log, project context
```

## Development workflow
See [docs/workflow-log.md](docs/workflow-log.md) for the slice-by-slice decisions and outcomes.
See [docs/acceptance-checklist.md](docs/acceptance-checklist.md) for the full requirements status.
See [docs/architecture.md](docs/architecture.md) for design decisions and component contracts.

