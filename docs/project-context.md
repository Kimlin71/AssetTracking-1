# Asset Tracking project context

## Goal
Create a C# console application that tracks company assets by office, type, purchase date, price, and currency.

## Required progression
### Level 1
- Model assets with classes and constructors.
- Include at least Computer and MobilePhone.
- Store instances in `List<Asset>`.

### Level 2
- Sort by asset type and purchase date.
- Use a three-year lifespan.
- Show end-of-life warnings according to the course specification.

### Level 3
- Support offices and office currencies.
- Support Sweden with SEK, USA with USD, and Turkey with TRY.
- Convert a USD price to local currency using either documented fixed rates or a live source.
- Sort by office and purchase date where required.

### Level 4
- Use an abstract base class or interface, inheritance, and polymorphism.
- Provide unique asset ID, age calculation, and a menu.

### Level 5, optional
- Save and load assets.
- Update persistence when assets are added or removed.
- Prefer JSON with `System.Text.Json`.
- Prevent duplicate asset IDs.

## Optional features
Search, edit, remove, pagination, colored console output, CSV export, exception handling, API-based conversion, and unit tests.

## Known design decision to resolve
The source material states both Yellow for less than three months remaining and Red for less than six months remaining. Since these ranges overlap, implement an explicit precedence rule and document it. Recommended interpretation: Red for three to six months remaining, Yellow for zero to three months remaining, and Expired after end of life. Confirm against instructor expectations before final submission.

## Existing reference implementations
The supplied material includes two different currency implementations based on European Central Bank XML. They should be treated as references, not copied blindly. Review duplicate-rate accumulation, missing currency handling, culture-safe number parsing, network failures, and use of `decimal` for money.

## Output expectations
Display clear aligned columns including office, asset type, brand, model, USD price, local price, purchase date, and status when applicable.
