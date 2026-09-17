using System.Globalization;
using AssetTracking;
using AssetTracking.Domain;
using AssetTracking.Services;

// Force dot as decimal separator for all number formatting throughout the process.
CultureInfo.DefaultThreadCurrentCulture   = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

// Always resolve the data file next to the executable so dotnet run and direct launch use the same file.
var DataFile = Path.Combine(AppContext.BaseDirectory, "assets.json");

// Fetch live exchange rates before doing anything else.
await CurrencyConverter.InitializeAsync();

// Load persisted assets; seed hardcoded data only on first run.
var assets = AssetRepository.Load(DataFile);
if (assets.Count == 0)
{
    assets.AddRange([
        new Computer    ("Dell",    "XPS 15",      new DateOnly(2023, 10, 16), 1200.00m, Office.Sweden),  // Yellow  (~1 month left)
        new MobilePhone ("Apple",   "iPhone 14",   new DateOnly(2024,  1, 16),  999.00m, Office.Sweden),  // Green   (~4 months left)
        new Computer    ("Apple",   "MacBook Pro", new DateOnly(2023,  8,  1), 1999.00m, Office.USA),     // Expired
        new MobilePhone ("Google",  "Pixel 8",     new DateOnly(2024,  8, 16),  699.00m, Office.USA),     // None    (~11 months left)
        new Computer    ("Lenovo",  "ThinkPad X1", new DateOnly(2024,  6, 16), 1350.00m, Office.Turkey),  // None    (~9 months left)
        new MobilePhone ("Samsung", "Galaxy S23",  new DateOnly(2023, 11, 16),  799.00m, Office.Turkey),  // Yellow  (~2 months left)
    ]);
    AssetRepository.Save(assets, DataFile);
}

bool running = true;
while (running)
{
    Console.WriteLine();
    Console.WriteLine("=== Asset Tracker ===");
    Console.WriteLine("1. Add Asset");
    Console.WriteLine("2. View Assets");
    Console.WriteLine("3. Sort Assets");
    Console.WriteLine("4. Search Asset");
    Console.WriteLine("5. Exit");
    Console.Write("Select option: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1":
            AddAsset(assets);
            break;

        case "2":
            AssetPrinter.PrintTable(assets
                .OrderBy(a => a.AssetType)
                .ThenBy(a => a.PurchaseDate));
            break;

        case "3":
            AssetPrinter.PrintTable(assets
                .OrderBy(a => a.Office)
                .ThenBy(a => a.PurchaseDate));
            break;

        case "4":
            SearchAssets(assets);
            break;

        case "5":
            running = false;
            break;

        default:
            Console.WriteLine("Invalid choice — enter 1 to 5.");
            break;
    }
}

void AddAsset(List<Asset> list)
{
    Console.WriteLine("(Type 'cancel' at any prompt to go back to the menu.)");

    var typeChoice = PromptHelper.AskChoice("Type (1=Computer, 2=MobilePhone): ", "1", "2");
    if (typeChoice is null) { Console.WriteLine("Cancelled — returning to menu."); return; }

    var brand = PromptHelper.Ask("Brand: ");
    if (brand is null) { Console.WriteLine("Cancelled — returning to menu."); return; }

    var model = PromptHelper.Ask("Model: ");
    if (model is null) { Console.WriteLine("Cancelled — returning to menu."); return; }

    DateOnly date;
    while (true)
    {
        var raw = PromptHelper.Ask("Purchase date (yyyy-MM-dd): ");
        if (raw is null) { Console.WriteLine("Cancelled — returning to menu."); return; }
        if (DateOnly.TryParse(raw, out date)) break;
        Console.WriteLine("  Invalid date. Use format yyyy-MM-dd, e.g. 2024-01-15.");
    }

    decimal price;
    while (true)
    {
        var raw = PromptHelper.Ask("Price in USD (e.g. 1200.50): ");
        if (raw is null) { Console.WriteLine("Cancelled — returning to menu."); return; }
        if (decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out price) && price >= 0)
            break;
        Console.WriteLine("  Invalid price. Enter a positive number.");
    }

    var officeChoice = PromptHelper.AskChoice("Office (1=Sweden, 2=USA, 3=Turkey): ", "1", "2", "3");
    if (officeChoice is null) { Console.WriteLine("Cancelled — returning to menu."); return; }

    // Switch expression maps the string choice to the Office enum value.
    // The discard _ is the default arm — reached only when officeChoice is "3" (Turkey).
    var office = officeChoice switch
    {
        "1" => Office.Sweden,
        "2" => Office.USA,
        _   => Office.Turkey,
    };

    // Ternary (?:) creates a Computer or MobilePhone based on what the user chose.
    Asset newAsset = typeChoice == "1"
        ? new Computer(brand, model, date, price, office)
        : new MobilePhone(brand, model, date, price, office);

    list.Add(newAsset);
    AssetRepository.Save(list, DataFile);  // persist immediately so the new asset survives a restart
    Console.WriteLine($"Asset {newAsset.Id} added and saved.");
}

void SearchAssets(List<Asset> list)
{
    Console.Write("Search keyword: ");
    // ?. safely handles null (e.g. when stdin is closed); ?? "" prevents a null reference later.
    var keyword = Console.ReadLine()?.Trim() ?? "";
    if (keyword.Length == 0) { Console.WriteLine("No keyword entered."); return; }

    // Collapse all whitespace so "Think Pad" matches "ThinkPad" and vice versa.
    var normalized = System.Text.RegularExpressions.Regex.Replace(keyword, @"\s+", "");

    bool Matches(string field)
    {
        var f = System.Text.RegularExpressions.Regex.Replace(field, @"\s+", "");
        return f.Contains(normalized, StringComparison.OrdinalIgnoreCase);
    }

    var matches = list.Where(a =>
        Matches(a.Brand) ||
        Matches(a.Model) ||
        Matches(a.AssetType) ||
        Matches(a.Office.ToString()) ||
        Matches(a.PurchaseDate.ToString("yyyy-MM-dd")));

    var results = matches.OrderBy(a => a.AssetType).ThenBy(a => a.PurchaseDate).ToList();

    if (results.Count == 0)
        Console.WriteLine("No assets match.");
    else
        AssetPrinter.PrintTable(results);
}
