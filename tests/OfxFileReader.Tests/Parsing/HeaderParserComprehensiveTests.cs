using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing;

namespace OfxFileReader.Tests.Parsing;

/// <summary>Comprehensive edge case tests for the OFX header parser.</summary>
public class HeaderParserComprehensiveTests
{
    /// <summary>Verifies that VERSION 200 with SGML data returns V2x.</summary>
    [Fact]
    public void Parse_Version200WithSgmlData_ReturnsV2x()
    {
        var content = "OFXHEADER:100\nDATA:OFXSGML\nVERSION:200\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
        Assert.False(result.IsXmlFormat);
    }

    /// <summary>Verifies that a minimal XML declaration header is parsed correctly.</summary>
    [Fact]
    public void Parse_XmlDeclarationMinimal_Works()
    {
        var content = """<?OFX VERSION="203"?><OFX>""";
        var result = HeaderParser.Parse(content);
        Assert.True(result.IsXmlDeclaration);
        Assert.True(result.IsXmlFormat);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
    }

    /// <summary>Verifies that a non-numeric OFXHEADER value defaults to zero.</summary>
    [Fact]
    public void Parse_NonNumericOfxHeader_DefaultsToZero()
    {
        var content = "OFXHEADER:ABC\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(0, result.Header.OfxHeaderValue);
    }

    /// <summary>Verifies that a missing OFXHEADER key applies default values.</summary>
    [Fact]
    public void Parse_MissingOfxHeaderKey_DefaultsApplied()
    {
        var content = "DATA:OFXSGML\nVERSION:102\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(0, result.Header.OfxHeaderValue);
        Assert.Equal("OFXSGML", result.Header.Data);
    }

    /// <summary>Verifies that when no OFX tag exists, the remaining content is the original.</summary>
    [Fact]
    public void Parse_NoOfxTag_RemainingContentIsOriginal()
    {
        var content = "OFXHEADER:100\nDATA:OFXSGML\nVERSION:102";
        var result = HeaderParser.Parse(content);
        Assert.Equal(content, result.RemainingContent);
    }

    /// <summary>Verifies that empty header values do not crash the parser.</summary>
    [Fact]
    public void Parse_EmptyHeaderValues_DoesNotCrash()
    {
        var content = "OFXHEADER:\nVERSION:\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(0, result.Header.OfxHeaderValue);
    }

    /// <summary>Verifies that DATA:OFXXML sets the IsXmlFormat flag.</summary>
    [Fact]
    public void Parse_DataOfxXml_IsXmlFormat()
    {
        var content = "OFXHEADER:100\nDATA:OFXXML\nVERSION:200\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.True(result.IsXmlFormat);
    }

    /// <summary>Verifies that VERSION 151 returns V1x.</summary>
    [Fact]
    public void Parse_Version151_ReturnsV1x()
    {
        var content = "OFXHEADER:100\nDATA:OFXSGML\nVERSION:151\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V1x, result.Header.Version);
    }

    /// <summary>Verifies that VERSION 220 returns V2x.</summary>
    [Fact]
    public void Parse_Version220_ReturnsV2x()
    {
        var content = "VERSION:220\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V2x, result.Header.Version);
    }

    /// <summary>Verifies that duplicate header keys use the last value.</summary>
    [Fact]
    public void Parse_DuplicateHeaderKeys_LastWins()
    {
        var content = "OFXHEADER:100\nOFXHEADER:200\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(200, result.Header.OfxHeaderValue);
    }

    /// <summary>Verifies that header values with extra whitespace are trimmed.</summary>
    [Fact]
    public void Parse_HeadersWithExtraWhitespace_TrimsValues()
    {
        var content = "VERSION:   102  \n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(OfxVersion.V1x, result.Header.Version);
    }

    /// <summary>Verifies that malformed header lines without colons are ignored.</summary>
    [Fact]
    public void Parse_MalformedLine_IgnoresLine()
    {
        var content = "OFXHEADER:100\nMALFORMED_LINE\nVERSION:102\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(100, result.Header.OfxHeaderValue);
        Assert.Equal(OfxVersion.V1x, result.Header.Version);
    }

    /// <summary>Verifies that UTF-8 charset (65001) is parsed correctly.</summary>
    [Fact]
    public void Parse_CharsetUtf8_ParsesCorrectly()
    {
        var content = "CHARSET:65001\nVERSION:102\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal(65001, result.Header.Charset);
    }

    /// <summary>Verifies that OLDFILEUID and NEWFILEUID are parsed correctly.</summary>
    [Fact]
    public void Parse_OldAndNewFileUid_NotEmpty()
    {
        var content = "OLDFILEUID:ABC123\nNEWFILEUID:DEF456\n\n<OFX>";
        var result = HeaderParser.Parse(content);
        Assert.Equal("ABC123", result.Header.OldFileUid);
        Assert.Equal("DEF456", result.Header.NewFileUid);
    }
}
