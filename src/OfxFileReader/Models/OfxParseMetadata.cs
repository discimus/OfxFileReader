namespace OfxFileReader.Models;

public sealed record OfxParseMetadata(
    DateTimeOffset ParsedAt,
    string ParserVersion,
    string DetectedEncoding,
    int TransactionCount,
    int StatementCount
);
