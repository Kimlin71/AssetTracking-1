namespace AssetTracking;

public static class PromptHelper
{
    // Returns null when the user types "cancel"; loops until non-empty input is given.
    public static string? Ask(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim() ?? "";

            if (input.Equals("cancel", StringComparison.OrdinalIgnoreCase))
                return null;

            if (input.Length > 0)
                return input;

            Console.WriteLine("  Cannot be empty.");
        }
    }

    // Like Ask, but rejects values not in the allowed set; prints an error and repeats on mismatch.
    public static string? AskChoice(string prompt, params string[] valid)
    {
        while (true)
        {
            var input = Ask(prompt);
            if (input is null)
                return null;

            if (Array.Exists(valid, v => v.Equals(input, StringComparison.OrdinalIgnoreCase)))
                return input;

            Console.WriteLine($"  Invalid choice. Enter one of: {string.Join(", ", valid)}.");
        }
    }
}
