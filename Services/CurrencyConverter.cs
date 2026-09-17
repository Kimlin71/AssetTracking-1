using System.Globalization;
using System.Text.Json;
using AssetTracking.Domain;

namespace AssetTracking.Services;

// static = no instance needed; call CurrencyConverter.InitializeAsync() directly.
public static class CurrencyConverter
{
    private const string  ApiKey       = "91553bdc61761d1a5e665868";  // student project — never commit a real key to a public repo
    private const decimal FallbackSek  = 10.50m;  // 1 USD = 10.50 SEK (offline fallback)
    private const decimal FallbackTry  = 32.00m;  // 1 USD = 32.00 TRY (offline fallback)

    // These fields are updated by InitializeAsync(); all other methods read from them.
    private static decimal _sekRate = FallbackSek;
    private static decimal _tryRate = FallbackTry;

    // async Task = this method does network I/O without blocking the rest of the program.
    // Fetches live rates once at startup; falls back to constants on any failure.
    public static async Task InitializeAsync()
    {
        try
        {
            using var client   = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var url            = $"https://v6.exchangerate-api.com/v6/{ApiKey}/latest/USD";
            var response       = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[Currency] API returned {(int)response.StatusCode} — using fallback rates.");
                return;
            }

            using var doc  = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var rates      = doc.RootElement.GetProperty("conversion_rates");

            _sekRate = ParseRate(rates, "SEK", FallbackSek);
            _tryRate = ParseRate(rates, "TRY", FallbackTry);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Currency] Could not reach API ({ex.Message}) — using fallback rates.");
        }
    }

    // Tries to read the named property from the JSON rates object and parse it as decimal.
    // Returns fallback if the property is missing or the text cannot be parsed.
    private static decimal ParseRate(JsonElement rates, string code, decimal fallback)
    {
        if (rates.TryGetProperty(code, out var el) &&
            decimal.TryParse(el.GetRawText(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return value;

        Console.WriteLine($"[Currency] Rate for {code} missing — using fallback.");
        return fallback;
    }

    // Multiplies the USD price by the cached exchange rate for the given office.
    // USA assets stay in USD (no conversion needed), so the switch returns usdPrice unchanged.
    public static decimal ToLocalCurrency(decimal usdPrice, Office office) => office switch
    {
        Office.Sweden => usdPrice * _sekRate,
        Office.Turkey => usdPrice * _tryRate,
        _             => usdPrice,
    };

    // Returns the three-letter ISO currency code for the office's local currency.
    public static string CurrencyCode(Office office) => office switch
    {
        Office.Sweden => "SEK",
        Office.Turkey => "TRY",
        _             => "USD",
    };
}
