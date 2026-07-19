namespace OfxFileReader.Models;

/// <summary>Metadata about the OFX parsing operation.</summary>
public sealed record OfxParseMetadata(
    /// <summary>The UTC timestamp when the document was parsed.</summary>
    DateTimeOffset ParsedAt,
    /// <summary>The version of the parser used.</summary>
    string ParserVersion,
    /// <summary>The encoding detected from the OFX header.</summary>
    string DetectedEncoding,
    /// <summary>The total number of transactions found across all statements.</summary>
    int TransactionCount,
    /// <summary>The total number of statements found across all account types.</summary>
    int StatementCount
);
