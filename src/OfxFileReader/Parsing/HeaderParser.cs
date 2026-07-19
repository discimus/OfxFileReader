using OfxFileReader.Models;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Exceptions;

namespace OfxFileReader.Parsing;

/// <summary>Result of parsing an OFX header, containing the header and remaining content.</summary>
internal sealed class OfxHeaderParseResult
{
    /// <summary>Gets the parsed OFX header information.</summary>
    public OfxHeader Header { get; init; } = null!;

    /// <summary>Gets the content remaining after the header section.</summary>
    public string RemainingContent { get; init; } = string.Empty;

    /// <summary>Gets whether the header was an XML-style declaration.</summary>
    public bool IsXmlDeclaration { get; init; }

    /// <summary>Gets whether the content is in XML format.</summary>
    public bool IsXmlFormat { get; init; }
}

/// <summary>Provides static methods for parsing OFX file headers.</summary>
internal static class HeaderParser
{
    /// <summary>Parses the OFX header from the beginning of the content string.</summary>
    public static OfxHeaderParseResult Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOfxHeaderException("File content is empty");

        var isXmlDeclaration = false;
        var headerFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var remainingContent = content;

        // Check for XML declaration header (OFX 2.x)
        var xmlDeclarationMatch = System.Text.RegularExpressions.Regex.Match(content,
            @"<\?OFX\s+(.*?)\?>", System.Text.RegularExpressions.RegexOptions.Singleline);

        if (xmlDeclarationMatch.Success)
        {
            isXmlDeclaration = true;
            var attrStr = xmlDeclarationMatch.Groups[1].Value;
            foreach (System.Text.RegularExpressions.Match attrMatch in System.Text.RegularExpressions.Regex.Matches(
                attrStr, @"(\w+)\s*=\s*""([^""]*)"""))
            {
                headerFields[attrMatch.Groups[1].Value] = attrMatch.Groups[2].Value;
            }
            remainingContent = content[xmlDeclarationMatch.Index..];
        }
        else
        {
            // SGML-style header: KEY:VALUE pairs before <OFX>
            var ofxIndex = content.IndexOf("<OFX", StringComparison.OrdinalIgnoreCase);
            var headerSection = ofxIndex > 0 ? content[..ofxIndex] : string.Empty;
            remainingContent = ofxIndex > 0 ? content[ofxIndex..] : content;

            foreach (var line in headerSection.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                var colonIndex = trimmedLine.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = trimmedLine[..colonIndex].Trim();
                    var value = trimmedLine[(colonIndex + 1)..].Trim();
                    if (!string.IsNullOrEmpty(key))
                    {
                        headerFields[key] = value;
                    }
                }
            }
        }

        var header = MapToHeader(headerFields);
        return new OfxHeaderParseResult
        {
            Header = header,
            RemainingContent = remainingContent,
            IsXmlDeclaration = isXmlDeclaration,
            IsXmlFormat = isXmlDeclaration || header.Data.Equals("OFXXML", StringComparison.OrdinalIgnoreCase)
        };
    }

    /// <summary>Maps a dictionary of header field key-value pairs to an <see cref="OfxHeader"/> record.</summary>
    private static OfxHeader MapToHeader(Dictionary<string, string> fields)
    {
        _ = int.TryParse(fields.GetValueOrDefault("OFXHEADER"), out var ofxHeaderValue);
        _ = int.TryParse(fields.GetValueOrDefault("CHARSET", "1252"), out var charset);

        var data = fields.GetValueOrDefault("DATA", "OFXSGML");
        var versionStr = fields.GetValueOrDefault("VERSION", "102");
        var security = fields.GetValueOrDefault("SECURITY", "NONE");
        var encoding = fields.GetValueOrDefault("ENCODING", "USASCII");
        var compression = fields.GetValueOrDefault("COMPRESSION", "NONE");
        var oldFileUid = fields.GetValueOrDefault("OLDFILEUID", "NONE");
        var newFileUid = fields.GetValueOrDefault("NEWFILEUID", "NONE");

        var version = DetermineVersion(ofxHeaderValue, versionStr, data);

        return new OfxHeader(ofxHeaderValue, data, version, security, encoding, charset,
            compression, oldFileUid, newFileUid);
    }

    /// <summary>Determines the OFX version from header values and data type.</summary>
    private static OfxVersion DetermineVersion(int ofxHeaderValue, string versionStr, string data)
    {
        if (ofxHeaderValue >= 200 || versionStr.StartsWith("2") ||
            data.Equals("OFXXML", StringComparison.OrdinalIgnoreCase))
        {
            return OfxVersion.V2x;
        }

        if (ofxHeaderValue > 0 || versionStr.StartsWith("1") ||
            data.Equals("OFXSGML", StringComparison.OrdinalIgnoreCase))
        {
            return OfxVersion.V1x;
        }

        return OfxVersion.Unknown;
    }

    /// <summary>Determines whether the content uses the XML OFX format.</summary>
    public static bool IsXmlContent(string content)
    {
        return content.Contains("<?OFX", StringComparison.OrdinalIgnoreCase) ||
               (content.Contains("OFXHEADER:", StringComparison.OrdinalIgnoreCase) == false &&
                content.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase));
    }
}
