using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Parsing.Converters;

internal static class SeverityTypeConverter
{
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
