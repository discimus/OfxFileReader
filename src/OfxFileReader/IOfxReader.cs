using OfxFileReader.Models;

namespace OfxFileReader;

/// <summary>Defines the contract for reading and parsing OFX financial files.</summary>
public interface IOfxReader
{
    /// <summary>Reads and parses an OFX file from the specified file path.</summary>
    OfxDocument Read(string filePath);

    /// <summary>Reads and parses an OFX file from the specified stream.</summary>
    OfxDocument Read(Stream stream);

    /// <summary>Reads and parses an OFX file from the specified text reader.</summary>
    OfxDocument Read(TextReader reader);

    /// <summary>Asynchronously reads and parses an OFX file from the specified file path.</summary>
    Task<OfxDocument> ReadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>Asynchronously reads and parses an OFX file from the specified stream.</summary>
    Task<OfxDocument> ReadAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>Asynchronously reads and parses an OFX file from the specified text reader.</summary>
    Task<OfxDocument> ReadAsync(TextReader reader, CancellationToken cancellationToken = default);
}
