using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

public class OfxAmountConverterComprehensiveTests
{
    [Theory]
    [InlineData("(45.50)", -45.50)]
    [InlineData("(1500.00)", -1500.00)]
    [InlineData("1,500.00", 1500.00)]
    [InlineData("  1500.00  ", 1500.00)]
    [InlineData("999999999999.99", 999999999999.99)]
    [InlineData("0.00", 0.00)]
    [InlineData("0", 0)]
    public void Parse_AdditionalFormats_ReturnsDecimal(string input, decimal expected)
    {
        var result = OfxAmountConverter.Parse(input);
        Assert.NotNull(result);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("15.00.00")]
    [InlineData("0x1A2B")]
    [InlineData("ABC")]
    [InlineData("--5.00")]
    [InlineData("+")]
    public void Parse_InvalidInput_ReturnsNull(string? input)
    {
        var result = OfxAmountConverter.Parse(input);
        Assert.Null(result);
    }
}
