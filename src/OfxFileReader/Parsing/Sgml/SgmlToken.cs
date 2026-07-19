namespace OfxFileReader.Parsing.Sgml;

/// <summary>Defines the types of tokens produced by the SGML tokenizer.</summary>
internal enum SgmlTokenType
{
    /// <summary>An opening tag (e.g., &lt;TAG&gt;).</summary>
    OpenTag,
    /// <summary>A closing tag (e.g., &lt;/TAG&gt;).</summary>
    CloseTag,
    /// <summary>Text content between tags.</summary>
    Text
}

/// <summary>Represents a single token from the SGML tokenization process.</summary>
/// <param name="Type">The type of token.</param>
/// <param name="Value">The token's value (tag name or text content).</param>
/// <param name="LineNumber">The line number where the token was found.</param>
internal sealed record SgmlToken(SgmlTokenType Type, string Value, int LineNumber);
