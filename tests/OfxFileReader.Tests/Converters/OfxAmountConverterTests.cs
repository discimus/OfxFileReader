using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

/// <summary>Tests for the OFX amount converter.</summary>
public class OfxAmountConverterTests
{
    [Theory]
    [InlineData("1500.00", 1500.00)]
    [InlineData("-45.50", -45.50)]
    [InlineData("0", 0)]
    [InlineData("1234567.89", 1234567.89)]
    /// <summary>Verifies that valid amount strings are parsed to decimal values.</summary>
    public void Parse_ValidAmounts_ReturnsDecimal(string input, decimal expected)
    {
        var result = OfxAmountConverter.Parse(input);
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]
    /// <summary>Verifies that invalid or empty input returns null.</summary>
    public void Parse_InvalidInput_ReturnsNull(string? input)
    {
        var result = OfxAmountConverter.Parse(input);
        Assert.Null(result);
    }
}
