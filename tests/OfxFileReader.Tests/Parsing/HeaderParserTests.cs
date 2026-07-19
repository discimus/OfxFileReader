using OfxFileReader.Parsing;

namespace OfxFileReader.Tests.Parsing;

/// <summary>Tests for the OFX header parser.</summary>
public class HeaderParserTests
{
    /// <summary>Verifies that a standard SGML-style header is parsed correctly.</summary>
    [Fact]
    public void Parse_SgmlHeader_ReturnsCorrectValues()
    {
        var content = """
            OFXHEADER:100
            DATA:OFXSGML
            VERSION:102
            SECURITY:NONE
            ENCODING:USASCII
            CHARSET:1252
            COMPRESSION:NONE
            OLDFILEUID:NONE
            NEWFILEUID:NONE

            <OFX>
            """;

        var result = HeaderParser.Parse(content);
        Assert.Equal(100, result.Header.OfxHeaderValue);
        Assert.Equal("OFXSGML", result.Header.Data);
        Assert.Equal(OfxVersion.V1x, result.Header.Version);
        Assert.Equal("NONE", result.Header.Security);
        Assert.Equal(1252, result.Header.Charset);
        Assert.False(result.IsXmlDeclaration);
        Assert.StartsWith("<OFX>", result.RemainingContent);
    }

    /// <summary>Verifies that an XML declaration-style header is parsed correctly.</summary>
    [Fact]
    public void Parse_XmlDeclarationHeader_ReturnsCorrectValues()
    {
        var content = """<?OFX OFXHEADER="200" VERSION="203" SECURITY="NONE" OLDFILEUID="NONE" NEWFILEUID="NONE"?><OFX>""";

        var result = HeaderParser.Parse(content);
        Assert.Equal(200, result.Header.OfxHeaderValue);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
        Assert.True(result.IsXmlDeclaration);
    }

    /// <summary>Verifies that empty content throws an <see cref="InvalidOfxHeaderException"/>.</summary>
    [Fact]
    public void Parse_EmptyContent_ThrowsException()
    {
        Assert.Throws<InvalidOfxHeaderException>(() => HeaderParser.Parse(""));
    }

    /// <summary>Verifies that a minimal header with only OFXHEADER returns default values for other fields.</summary>
    [Fact]
    public void Parse_MinimalHeader_ReturnsDefaults()
    {
        var content = "OFXHEADER:100\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(100, result.Header.OfxHeaderValue);
        Assert.Equal("OFXSGML", result.Header.Data);
    }
}
