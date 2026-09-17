using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using AssetTracking.Domain;

namespace AssetTracking.Services;

// file = this class is only visible inside this source file; it's an implementation detail.
// System.Text.Json doesn't know how to read/write DateOnly by default, so we teach it here.
file sealed class DateOnlyConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.Parse(reader.GetString()!, CultureInfo.InvariantCulture);

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
}

public static class AssetRepository
{
    // Shared serializer settings: pretty-print JSON, camelCase keys, and our custom DateOnly converter.
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented        = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters           = { new DateOnlyConverter() },
    };

    // Reads assets from the JSON file. Returns an empty list if the file doesn't exist yet.
    public static List<Asset> Load(string filePath)
    {
        if (!File.Exists(filePath))
            return [];

        var json   = File.ReadAllText(filePath);
        List<Asset> loaded;
        try
        {
            loaded = JsonSerializer.Deserialize<List<Asset>>(json, Options) ?? [];
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[Repository] Could not read {filePath}: {ex.Message}");
            Console.WriteLine("[Repository] Starting with empty list.");
            return [];
        }

        // Reject duplicate IDs — keep only the first occurrence of each ID.
        // HashSet<int>.Add returns false when the value is already in the set.
        var seen   = new HashSet<int>();
        var result = new List<Asset>();
        foreach (var asset in loaded)
        {
            if (seen.Add(asset.Id))
                result.Add(asset);
            else
                Console.WriteLine($"[Repository] Duplicate ID {asset.Id} skipped.");
        }

        if (result.Count > 0)
        {
            // Seed the auto-increment counter so new assets get IDs above the highest saved ID.
            int maxId = result.Max(a => a.Id);
            Asset.ResetIdCounter(maxId + 1);
        }

        return result;
    }

    public static void Save(List<Asset> assets, string filePath)
    {
        var json = JsonSerializer.Serialize<IEnumerable<Asset>>(assets, Options);

        // Write to a temp file then replace, so a crash mid-write doesn't corrupt the saved data.
        var tmp = filePath + ".tmp";
        File.WriteAllText(tmp, json);
        File.Move(tmp, filePath, overwrite: true);
    }
}
