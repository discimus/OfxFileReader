using OfxFileReader.Models;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Exceptions;

namespace OfxFileReader.Parsing;

internal sealed class OfxHeaderParseResult
{
    public OfxHeader Header { get; init; } = null!;
    public string RemainingContent { get; init; } = string.Empty;
    public bool IsXmlDeclaration { get; init; }
    public bool IsXmlFormat { get; init; }
}

internal static class HeaderParser
{
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

    public static bool IsXmlContent(string content)
    {
        return content.Contains("<?OFX", StringComparison.OrdinalIgnoreCase) ||
               (content.Contains("OFXHEADER:", StringComparison.OrdinalIgnoreCase) == false &&
                content.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase));
    }
}
