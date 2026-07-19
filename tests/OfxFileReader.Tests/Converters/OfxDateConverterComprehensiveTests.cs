using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

public class OfxDateConverterComprehensiveTests
{
    [Theory]
    [InlineData("20241015[-5:EST]", "2024-10-15T00:00:00-05:00")]
    [InlineData("20241015143022[EST]", "2024-10-15T14:30:22-05:00")]
    [InlineData("20241015143022.000[UTC]", "2024-10-15T14:30:22+00:00")]
    [InlineData("20241015143022[+2]", "2024-10-15T14:30:22+02:00")]
    [InlineData("20241015143022[GMT]", "2024-10-15T14:30:22+00:00")]
    [InlineData("20241015143022[]", "2024-10-15T14:30:22+00:00")]
    [InlineData("20241015143022.", "2024-10-15T14:30:22+00:00")]
    [InlineData("2024101514", null)]  // 10-digit YYYYMMDDHH not supported
    [InlineData("20241015143022.000[+0:UTC]", "2024-10-15T14:30:22+00:00")]
    [InlineData("20241015143022[-7:PDT]", "2024-10-15T14:30:22-07:00")]
    public void Parse_VariousFormats_ReturnsExpected(string? input, string? expected)
    {
        var result = OfxDateConverter.Parse(input);
        if (expected is null)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Equal(DateTimeOffset.Parse(expected), result.Value);
        }
    }

    [Fact]
    public void Parse_InvalidDate_ReturnsNull()
    {
        var result = OfxDateConverter.Parse("20240230");
        Assert.Null(result);
    }

    [Fact]
    public void Parse_MultipleBrackets_ParsesFirstOnly()
    {
        // Edge case: content after the bracket pair is ignored
        var result = OfxDateConverter.Parse("20241015143022.000[-5][EXTRA]");
        Assert.NotNull(result);
        Assert.Equal(-5, result.Value.Offset.Hours);
    }

    [Fact]
    public void Parse_PositiveTimezone_Works()
    {
        var result = OfxDateConverter.Parse("20241015143022[+2]");
        Assert.NotNull(result);
        Assert.Equal(2, result.Value.Offset.Hours);
    }

    [Fact]
    public void Parse_ZeroTimezone_Works()
    {
        var result = OfxDateConverter.Parse("20241015143022[+0:UTC]");
        Assert.NotNull(result);
        Assert.Equal(0, result.Value.Offset.Hours);
    }
}
