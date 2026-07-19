namespace OfxFileReader.Parsing.Sgml;

internal enum SgmlTokenType
{
    OpenTag,
    CloseTag,
    Text
}

internal sealed record SgmlToken(SgmlTokenType Type, string Value, int LineNumber);
