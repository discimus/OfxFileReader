using System.Globalization;
using System.Text.RegularExpressions;

namespace OfxFileReader.Parsing.Converters;

/// <summary>Provides helper methods for converting OFX date strings to <see cref="DateTimeOffset"/> values.</summary>
internal static partial class OfxDateConverter
{
    private static readonly Dictionary<string, int> KnownTimeZones = new(StringComparer.OrdinalIgnoreCase)
    {
        ["EST"] = -5, ["EDT"] = -4,
        ["CST"] = -6, ["CDT"] = -5,
        ["MST"] = -7, ["MDT"] = -6,
        ["PST"] = -8, ["PDT"] = -7,
        ["GMT"] = 0,  ["UTC"] = 0
    };

    /// <summary>Regex for timezone format with optional offset and name (e.g., -5:EST).</summary>
    [GeneratedRegex(@"^([+-]?\d{1,2}):([A-Za-z]+)$")]
    private static partial Regex TimezoneWithNameRegex();

    /// <summary>Regex for timezone numeric offset (e.g., -5, +2).</summary>
    [GeneratedRegex(@"^[+-]?\d{1,2}$")]
    private static partial Regex TimezoneOffsetRegex();

    /// <summary>Parses an OFX date string into a <see cref="DateTimeOffset"/>.</summary>
    public static DateTimeOffset? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        value = value.Trim();

        try
        {
            var tzOffset = TimeSpan.Zero;
            var dateTimePart = value;

            if (value.Contains('[') && value.Contains(']'))
            {
                var bracketStart = value.IndexOf('[');
                var bracketEnd = value.IndexOf(']', bracketStart);
                if (bracketStart > 0 && bracketEnd > bracketStart)
                {
                    dateTimePart = value[..bracketStart].TrimEnd('.');
                    var tzPart = value[(bracketStart + 1)..bracketEnd];

                    tzOffset = ParseTimezonePart(tzPart);
                }
            }

            // Remove fractional seconds (e.g., .000)
            var dotIndex = dateTimePart.IndexOf('.');
            if (dotIndex > 0)
                dateTimePart = dateTimePart[..dotIndex];

            string[] formats = [
                "yyyyMMddHHmmss",
                "yyyyMMddHHmm",
                "yyyyMMdd"
            ];

            if (DateTime.TryParseExact(dateTimePart, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return new DateTimeOffset(dt, tzOffset);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Parses a timezone string into a <see cref="TimeSpan"/> offset.</summary>
    private static TimeSpan ParseTimezonePart(string tzPart)
    {
        if (string.IsNullOrWhiteSpace(tzPart))
            return TimeSpan.Zero;

        var match = TimezoneWithNameRegex().Match(tzPart);
        if (match.Success)
        {
            var offsetStr = match.Groups[1].Value;
            var tzName = match.Groups[2].Value;

            if (int.TryParse(offsetStr, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var hours))
            {
                return TimeSpan.FromHours(hours);
            }

            if (KnownTimeZones.TryGetValue(tzName, out var knownOffset))
            {
                return TimeSpan.FromHours(knownOffset);
            }
        }

        if (TimezoneOffsetRegex().IsMatch(tzPart))
        {
            if (int.TryParse(tzPart, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var hours))
            {
                return TimeSpan.FromHours(hours);
            }
        }

        if (KnownTimeZones.TryGetValue(tzPart, out var tzOffset))
        {
            return TimeSpan.FromHours(tzOffset);
        }

        return TimeSpan.Zero;
    }
}
