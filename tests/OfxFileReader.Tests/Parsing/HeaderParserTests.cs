using OfxFileReader.Parsing;

namespace OfxFileReader.Tests.Parsing;

public class HeaderParserTests
{
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

    [Fact]
    public void Parse_XmlDeclarationHeader_ReturnsCorrectValues()
    {
        var content = """<?OFX OFXHEADER="200" VERSION="203" SECURITY="NONE" OLDFILEUID="NONE" NEWFILEUID="NONE"?><OFX>""";

        var result = HeaderParser.Parse(content);
        Assert.Equal(200, result.Header.OfxHeaderValue);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
        Assert.True(result.IsXmlDeclaration);
    }

    [Fact]
    public void Parse_EmptyContent_ThrowsException()
    {
        Assert.Throws<InvalidOfxHeaderException>(() => HeaderParser.Parse(""));
    }

    [Fact]
    public void Parse_MinimalHeader_ReturnsDefaults()
    {
        var content = "OFXHEADER:100\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(100, result.Header.OfxHeaderValue);
        Assert.Equal("OFXSGML", result.Header.Data);
    }
}
