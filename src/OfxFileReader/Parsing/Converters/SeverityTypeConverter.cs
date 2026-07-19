using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Parsing.Converters;

/// <summary>Converts OFX severity strings to <see cref="SeverityType"/> enum values.</summary>
internal static class SeverityTypeConverter
{
    /// <summary>Parses an OFX severity string into a <see cref="SeverityType"/>.</summary>
    public static SeverityType Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return SeverityType.Unknown;

        return value.Trim().ToUpperInvariant() switch
        {
            "INFO" => SeverityType.Info,
            "WARN" => SeverityType.Warn,
            "ERROR" => SeverityType.Error,
            _ => SeverityType.Unknown
        };
    }
}
