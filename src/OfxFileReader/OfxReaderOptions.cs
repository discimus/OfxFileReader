using System.Text;
using OfxFileReader.Logging;

namespace OfxFileReader;

public sealed record OfxReaderOptions
{
    public static readonly OfxReaderOptions Default = new();

    public Encoding Encoding { get; init; } = Encoding.UTF8;
    public bool StrictMode { get; init; } = false;
    public IOfxLogger? Logger { get; init; } = null;
}
