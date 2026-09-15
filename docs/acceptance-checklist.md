# Acceptance checklist

## Mandatory
- [ ] Does the solution contain a C# console application?
- [ ] Is there a shared Asset abstraction?
- [ ] Are Computer and MobilePhone represented as separate concrete types?
- [ ] Are assets stored in a `List<Asset>` or an equivalent collection of the base type?
- [ ] Does every asset contain brand, model, purchase date, USD price, local price, office, and asset type?
- [ ] Can assets be sorted according to the required level?
- [ ] Is the three-year lifespan calculated from the purchase date?
- [ ] Are warning states displayed with documented, non-overlapping rules?
- [ ] Does each office have the correct local currency?
- [ ] Are prices displayed in both USD and local currency?

## Advanced
- [ ] Does the solution demonstrate inheritance and polymorphism?
- [ ] Does each asset have a unique ID?
- [ ] Is asset age calculated?
- [ ] Is there a usable menu?

## Optional persistence
- [ ] Are assets loaded when the application starts?
- [ ] Are changes saved after add or remove operations?
- [ ] Are duplicate IDs prevented?
- [ ] Can an asset report be exported?
