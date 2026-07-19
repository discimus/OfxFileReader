# OfxFileReader - Test Conventions

## Framework

- xUnit v2
- No mocking framework (pure assertions)
- Integration tests use real fixture files

## Structure

- `Converters/` - Unit tests for converters
- `Parsing/` - Tokenizer, TreeBuilder, HeaderParser tests
- `Mappers/` - SgmlNode -> Model tests (construct nodes manually)
- `Integration/` - End-to-end with fixture files
- `Fixtures/` - Real .ofx files (SGML + XML)

## Rules

- NEVER modify existing tests
- NEW test file for new scenarios
- One assertion concept per test
- Test names: {Method}_{Scenario}_ExpectedResult

## Fixtures

- Add new .ofx file to `Fixtures/`
- Set CopyToOutputDirectory = PreserveNewest
- Use existing fixtures as templates
