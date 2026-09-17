using System.Globalization;
using AssetTracking.Domain;
using AssetTracking.Services;

namespace AssetTracking;

// Responsible only for writing the asset table to the console — no business logic here.
public static class AssetPrinter
{
    // dot-thousands, comma-decimal (SEK, TRY)
    private static readonly NumberFormatInfo DotThousandsFormat = new()
    {
        NumberGroupSeparator  = ".",
        NumberDecimalSeparator = ",",
        NumberGroupSizes       = [3],
    };

    // comma-thousands, dot-decimal (USD)
    private static readonly NumberFormatInfo CommaThousandsFormat = new()
    {
        NumberGroupSeparator  = ",",
        NumberDecimalSeparator = ".",
        NumberGroupSizes       = [3],
    };

    public static void PrintTable(IEnumerable<Asset> assets)
    {
        // Each {N,width}: negative = left-align, positive = right-align.
        const string fmt = "{0,-4} {1,-10} {2,-13} {3,-10} {4,-15} {5,14} {6,-9} {7}";

        Console.ResetColor();
        Console.WriteLine(fmt,
            "ID", "Office", "Type", "Brand", "Model",
            "Local Price", "Currency", "Purchase Date");
        Console.WriteLine(new string('-', 90));

        foreach (var asset in assets)
        {
            Console.ResetColor();
            // Green = AssetStatus.Red ("getting close" — plan replacement); Red = Expired; Yellow = act soon.
            if      (asset.Status == AssetStatus.Yellow)  Console.ForegroundColor = ConsoleColor.Yellow;
            else if (asset.Status == AssetStatus.Red)     Console.ForegroundColor = ConsoleColor.Green;
            else if (asset.Status == AssetStatus.Expired) Console.ForegroundColor = ConsoleColor.Red;

            var currency   = CurrencyConverter.CurrencyCode(asset.Office);
            var localPrice = CurrencyConverter.ToLocalCurrency(asset.PriceUsd, asset.Office);
            var fmt2       = asset.Office == Office.USA ? CommaThousandsFormat : DotThousandsFormat;

            Console.WriteLine(fmt,
                asset.Id,
                asset.Office,
                asset.AssetType,
                asset.Brand,
                asset.Model,
                localPrice.ToString("N2", fmt2),
                currency,
                asset.PurchaseDate.ToString("yyyy-MM-dd"));
        }

        Console.ResetColor();
    }
}
