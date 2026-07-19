# OfxFileReader - AI Context

.NET 8 class library for parsing OFX financial files.

## Commands

```bash
dotnet build              # Build solution
dotnet test               # Run all tests (199+ tests)
dotnet test --filter "Category=Unit"   # Unit tests only
```

## Conventions

- C# 12, .NET 8, nullable enabled
- Records for domain models (immutable)
- Internal for implementation details
- Tests: xUnit + fluent assertions
- Namespace: `OfxFileReader.* -> Tests: *Tests`

## Key Files

- `src/OfxFileReader/OfxReader.cs` - Entry point
- `src/OfxFileReader/Models/OfxDocument.cs` - Output model
- `src/OfxFileReader/Parsing/Sgml/SgmlTokenizer.cs` - SGML parser
- `src/OfxFileReader/Parsing/Xml/XmlOfxParser.cs` - XML parser

## Rules

- NEVER change existing tests - create new ones
- Errors -> `OfxParseException` hierarchy
- Monetary values -> `decimal`
- Dates -> `DateTimeOffset`
- Never log FITID (banking privacy)
- 199+ tests must pass before commit
