using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing;

namespace OfxFileReader.Tests.Parsing;

public class HeaderParserComprehensiveTests
{
    [Fact]
    public void Parse_Version200WithSgmlData_ReturnsV2x()
    {
        var content = "OFXHEADER:100\nDATA:OFXSGML\nVERSION:200\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
        Assert.False(result.IsXmlFormat);
    }

    [Fact]
    public void Parse_XmlDeclarationMinimal_Works()
    {
        var content = """<?OFX VERSION="203"?><OFX>""";
        var result = HeaderParser.Parse(content);
        Assert.True(result.IsXmlDeclaration);
        Assert.True(result.IsXmlFormat);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
    }

    [Fact]
    public void Parse_NonNumericOfxHeader_DefaultsToZero()
    {
        var content = "OFXHEADER:ABC\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(0, result.Header.OfxHeaderValue);
    }

    [Fact]
    public void Parse_MissingOfxHeaderKey_DefaultsApplied()
    {
        var content = "DATA:OFXSGML\nVERSION:102\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(0, result.Header.OfxHeaderValue);
        Assert.Equal("OFXSGML", result.Header.Data);
    }

    [Fact]
    public void Parse_NoOfxTag_RemainingContentIsOriginal()
    {
        var content = "OFXHEADER:100\nDATA:OFXSGML\nVERSION:102";
        var result = HeaderParser.Parse(content);
        Assert.Equal(content, result.RemainingContent);
    }

    [Fact]
    public void Parse_EmptyHeaderValues_DoesNotCrash()
    {
        var content = "OFXHEADER:\nVERSION:\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(0, result.Header.OfxHeaderValue);
    }

    [Fact]
    public void Parse_DataOfxXml_IsXmlFormat()
    {
        var content = "OFXHEADER:100\nDATA:OFXXML\nVERSION:200\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.True(result.IsXmlFormat);
    }

    [Fact]
    public void Parse_Version151_ReturnsV1x()
    {
        var content = "OFXHEADER:100\nDATA:OFXSGML\nVERSION:151\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V1x, result.Header.Version);
    }

    [Fact]
    public void Parse_Version220_ReturnsV2x()
    {
        var content = "VERSION:220\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
    }

    [Fact]
    public void Parse_DuplicateHeaderKeys_LastWins()
    {
        var content = "OFXHEADER:100\nOFXHEADER:200\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(200, result.Header.OfxHeaderValue);
    }

    [Fact]
    public void Parse_HeadersWithExtraWhitespace_TrimsValues()
    {
        var content = "VERSION:   102  \n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V1x, result.Header.Version);
    }

    [Fact]
    public void Parse_MalformedLine_IgnoresLine()
    {
        var content = "OFXHEADER:100\nMALFORMED_LINE\nVERSION:102\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(100, result.Header.OfxHeaderValue);
        Assert.Equal(OfxVersion.V1x, result.Header.Version);
    }

    [Fact]
    public void Parse_CharsetUtf8_ParsesCorrectly()
    {
        var content = "CHARSET:65001\nVERSION:102\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(65001, result.Header.Charset);
    }

    [Fact]
    public void Parse_OldAndNewFileUid_NotEmpty()
    {
        var content = "OLDFILEUID:ABC123\nNEWFILEUID:DEF456\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal("ABC123", result.Header.OldFileUid);
        Assert.Equal("DEF456", result.Header.NewFileUid);
    }
}
