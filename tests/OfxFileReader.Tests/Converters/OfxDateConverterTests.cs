using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

/// <summary>Tests for the OFX date converter.</summary>
public class OfxDateConverterTests
{
    [Theory]
    [InlineData("20241015", "2024-10-15T00:00:00+00:00")]
    [InlineData("20241015143022", "2024-10-15T14:30:22+00:00")]
    [InlineData("20241015143022.000[-5:EST]", "2024-10-15T14:30:22-05:00")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    /// <summary>Verifies that various OFX date formats are parsed correctly.</summary>
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

    /// <summary>Verifies that timezone offsets are parsed correctly.</summary>
    [Fact]
    public void Parse_WithTimezoneOffset_ReturnsCorrectOffset()
    {
        var result = OfxDateConverter.Parse("20241015143022.000[-5:EST]");
        Assert.NotNull(result);
        Assert.Equal(-5, result.Value.Offset.Hours);
    }

    /// <summary>Verifies that invalid date strings return null.</summary>
    [Fact]
    public void Parse_InvalidDate_ReturnsNull()
    {
        var result = OfxDateConverter.Parse("not-a-date");
        Assert.Null(result);
    }
}
