# OfxFileReader - Source Conventions

## Structure

- `Models/` - Immutable records (new = add to OfxDocument)
- `Parsing/` - Strategy: SgmlOfxParser | XmlOfxParser
- `Parsing/Mappers/` - SgmlNode -> Model mapping
- `Parsing/Converters/` - Date, Amount, Enum converters
- `Exceptions/` - OfxParseException hierarchy
- `Logging/` - IOfxLogger interface

## Patterns

- All internal classes (internal sealed)
- Static mappers/converters (pure functions)
- Records with init properties
- Nullable reference types enabled

## Adding new message set

1. Create Model in `Models/` (record)
2. Add to `OfxDocument` record
3. Create Mapper in `Parsing/Mappers/`
4. Add parsing in `SgmlOfxParser.cs`
5. Add parsing in `XmlOfxParser.cs`
6. Add validation in `OfxValidator.cs`
7. Add tests in test project

## Conventions

- No comments in production code
- No regions
- One type per file
- File-scoped namespaces
