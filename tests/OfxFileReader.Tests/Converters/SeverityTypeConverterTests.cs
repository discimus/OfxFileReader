using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

/// <summary>Tests for the severity type converter.</summary>
public class SeverityTypeConverterTests
{
    [Theory]
    [InlineData("INFO", SeverityType.Info)]
    [InlineData("WARN", SeverityType.Warn)]
    [InlineData("ERROR", SeverityType.Error)]
    [InlineData("info", SeverityType.Info)]
    [InlineData("Info", SeverityType.Info)]
    [InlineData("Warn", SeverityType.Warn)]
    /// <summary>Verifies that valid severity strings map to the correct enum values.</summary>
    public void Parse_ValidInput_ReturnsCorrectEnum(string input, SeverityType expected)
    {
        var result = SeverityTypeConverter.Parse(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("INVALID")]
    [InlineData("WARNING")]
    /// <summary>Verifies that invalid or empty input returns Unknown.</summary>
    public void Parse_InvalidInput_ReturnsUnknown(string? input)
    {
        var result = SeverityTypeConverter.Parse(input);
        Assert.Equal(SeverityType.Unknown, result);
    }
}
