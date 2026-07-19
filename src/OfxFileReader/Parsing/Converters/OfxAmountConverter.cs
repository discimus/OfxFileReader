using System.Globalization;

namespace OfxFileReader.Parsing.Converters;

/// <summary>Provides helper methods for converting OFX amount strings to <see cref="decimal"/> values.</summary>
internal static class OfxAmountConverter
{
    /// <summary>Parses an OFX amount string into a nullable decimal.</summary>
    public static decimal? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim();

        // GAAP standard: parentheses indicate negative amounts
        if (value.Length > 2 && value[0] == '(' && value[^1] == ')')
        {
            value = "-" + value[1..^1];
        }

        if (decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }
}
