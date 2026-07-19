using OfxFileReader.Models;

namespace OfxFileReader;

public interface IOfxReader
{
    OfxDocument Read(string filePath);
    OfxDocument Read(Stream stream);
    OfxDocument Read(TextReader reader);
    Task<OfxDocument> ReadAsync(string filePath, CancellationToken cancellationToken = default);
    Task<OfxDocument> ReadAsync(Stream stream, CancellationToken cancellationToken = default);
    Task<OfxDocument> ReadAsync(TextReader reader, CancellationToken cancellationToken = default);
}
