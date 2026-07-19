using System.Text;
using OfxFileReader.Logging;

namespace OfxFileReader;

/// <summary>Configuration options for the OFX reader.</summary>
public sealed record OfxReaderOptions
{
    /// <summary>Default instance with standard settings (UTF-8 encoding, non-strict mode, no logger).</summary>
    public static readonly OfxReaderOptions Default = new();

    /// <summary>Gets the encoding used when reading the OFX file content.</summary>
    public Encoding Encoding { get; init; } = Encoding.UTF8;

    /// <summary>Gets a value indicating whether strict parsing mode is enabled.</summary>
    public bool StrictMode { get; init; } = false;

    /// <summary>Gets the logger instance used for diagnostic messages during parsing.</summary>
    public IOfxLogger? Logger { get; init; } = null;
}
