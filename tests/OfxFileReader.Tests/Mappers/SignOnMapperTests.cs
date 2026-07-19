using OfxFileReader.Logging;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Mappers;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Tests.Mappers;

/// <summary>Tests for the sign-on response SGML mapper.</summary>
public class SignOnMapperTests
{
    private static readonly IOfxLogger Logger = NullOfxLogger.Instance;

    /// <summary>Helper method to create an SGML node with an optional value.</summary>
    private static SgmlNode CreateNode(string name, string? value = null)
    {
        var node = new SgmlNode(name);
        if (value is not null)
            node.Value = value;
        return node;
    }

    /// <summary>Verifies that a complete valid SONRS node maps correctly.</summary>
    [Fact]
    public void Map_CompleteValid_ReturnsSignOn()
    {
        var sonrs = new SgmlNode("SONRS");
        var status = new SgmlNode("STATUS");
        status.Children.Add(CreateNode("CODE", "0"));
        status.Children.Add(CreateNode("SEVERITY", "INFO"));
        status.Children.Add(CreateNode("MESSAGE", "OK"));
        sonrs.Children.Add(status);
        sonrs.Children.Add(CreateNode("DTSERVER", "20241015143022"));
        sonrs.Children.Add(CreateNode("LANGUAGE", "ENG"));

        var fi = new SgmlNode("FI");
        fi.Children.Add(CreateNode("ORG", "TESTBANK"));
        fi.Children.Add(CreateNode("FID", "000111222"));
        sonrs.Children.Add(fi);
        sonrs.Children.Add(CreateNode("SESSCOOKIE", "ABC123"));

        var result = SignOnMapper.Map(sonrs, Logger);

        Assert.NotNull(result);
        Assert.Equal(0, result.Status.Code);
        Assert.Equal(SeverityType.Info, result.Status.Severity);
        Assert.Equal("OK", result.Status.Message);
        Assert.NotEqual(DateTimeOffset.MinValue, result.ServerDate);
        Assert.Equal("ENG", result.Language);
        Assert.NotNull(result.FinancialInstitution);
        Assert.Equal("TESTBANK", result.FinancialInstitution.Organization);
        Assert.Equal("000111222", result.FinancialInstitution.Fid);
        Assert.Equal("ABC123", result.SessionCookie);
    }

    /// <summary>Verifies that a missing STATUS node uses default values.</summary>
    [Fact]
    public void Map_MissingStatus_UsesDefaults()
    {
        var sonrs = new SgmlNode("SONRS");
        sonrs.Children.Add(CreateNode("DTSERVER", "20241015143022"));

        var result = SignOnMapper.Map(sonrs, Logger);

        Assert.NotNull(result);
        Assert.Equal(0, result.Status.Code);
        Assert.Equal(SeverityType.Info, result.Status.Severity);
    }

    /// <summary>Verifies that a missing FI node results in a null financial institution.</summary>
    [Fact]
    public void Map_MissingFi_FinancialInstitutionIsNull()
    {
        var sonrs = new SgmlNode("SONRS");
        sonrs.Children.Add(CreateNode("DTSERVER", "20241015143022"));
        sonrs.Children.Add(CreateNode("LANGUAGE", "ENG"));

        var result = SignOnMapper.Map(sonrs, Logger);

        Assert.NotNull(result);
        Assert.Null(result.FinancialInstitution);
    }

    /// <summary>Verifies that error status codes are parsed correctly.</summary>
    [Fact]
    public void Map_ErrorStatus_ParsesCorrectly()
    {
        var sonrs = new SgmlNode("SONRS");
        var status = new SgmlNode("STATUS");
        status.Children.Add(CreateNode("CODE", "2000"));
        status.Children.Add(CreateNode("SEVERITY", "ERROR"));
        status.Children.Add(CreateNode("MESSAGE", "Invalid credentials"));
        sonrs.Children.Add(status);
        sonrs.Children.Add(CreateNode("DTSERVER", "20241015143022"));

        var result = SignOnMapper.Map(sonrs, Logger);

        Assert.NotNull(result);
        Assert.Equal(2000, result.Status.Code);
        Assert.Equal(SeverityType.Error, result.Status.Severity);
        Assert.Equal("Invalid credentials", result.Status.Message);
    }
}
