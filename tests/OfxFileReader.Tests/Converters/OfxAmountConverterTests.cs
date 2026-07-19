using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

public class OfxAmountConverterTests
{
    [Theory]
    [InlineData("1500.00", 1500.00)]
    [InlineData("-45.50", -45.50)]
    [InlineData("0", 0)]
    [InlineData("1234567.89", 1234567.89)]
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
    public void Parse_InvalidInput_ReturnsNull(string? input)
    {
        var result = OfxAmountConverter.Parse(input);
        Assert.Null(result);
    }
}
