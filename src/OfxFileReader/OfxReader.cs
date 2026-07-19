using OfxFileReader.Exceptions;
using OfxFileReader.Logging;
using OfxFileReader.Models;
using OfxFileReader.Parsing;
using OfxFileReader.Parsing.Sgml;
using OfxFileReader.Parsing.Xml;

namespace OfxFileReader;

public sealed class OfxReader : IOfxReader
{
    private readonly OfxReaderOptions _options;
    private readonly IOfxLogger _logger;

    public OfxReader()
        : this(OfxReaderOptions.Default)
    {
    }

    public OfxReader(OfxReaderOptions options)
    {
        _options = options ?? OfxReaderOptions.Default;
        _logger = options.Logger ?? NullOfxLogger.Instance;
    }

    public OfxDocument Read(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"OFX file not found: {filePath}", filePath);

        var content = File.ReadAllText(filePath, _options.Encoding);
        return ParseContent(content);
    }

    public OfxDocument Read(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new StreamReader(stream, _options.Encoding);
        var content = reader.ReadToEnd();
        return ParseContent(content);
    }

    public OfxDocument Read(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var content = reader.ReadToEnd();
        return ParseContent(content);
    }

    public async Task<OfxDocument> ReadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"OFX file not found: {filePath}", filePath);

        var content = await File.ReadAllTextAsync(filePath, _options.Encoding, cancellationToken).ConfigureAwait(false);
        return ParseContent(content);
    }

    public async Task<OfxDocument> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var reader = new StreamReader(stream, _options.Encoding);
        var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ParseContent(content);
    }

    public async Task<OfxDocument> ReadAsync(TextReader reader, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var content = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
        return ParseContent(content);
    }

    private OfxDocument ParseContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOfxHeaderException("OFX content is empty");

        var headerResult = TryParseHeader(content, out _);

        OfxDocument document;

        if (headerResult.IsXmlFormat)
        {
            var xmlParser = new XmlOfxParser(_logger);
            document = xmlParser.Parse(content);
            _logger.LogInformation("Successfully parsed OFX content using XML parser");
            return document;
        }

        try
        {
            var sgmlParser = new SgmlOfxParser(_logger);
            document = sgmlParser.Parse(content);
            _logger.LogInformation("Successfully parsed OFX content using SGML parser");
        }
        catch (Exception sgmlEx)
        {
            _logger.LogDebug($"SGML parser failed, trying XML fallback: {sgmlEx.Message}");

            try
            {
                var xmlParser = new XmlOfxParser(_logger);
                document = xmlParser.Parse(content);
                _logger.LogInformation("Successfully parsed OFX content using XML parser (fallback)");
            }
            catch (Exception xmlEx)
            {
                throw new OfxParseException(
                    $"Failed to parse OFX content. SGML error: {sgmlEx.Message}. XML error: {xmlEx.Message}",
                    xmlEx);
            }
        }

        return document;
    }

    private static OfxHeaderParseResult TryParseHeader(string content, out OfxHeader header)
    {
        var result = HeaderParser.Parse(content);
        header = result.Header;
        return result;
    }
}
