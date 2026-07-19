namespace OfxFileReader.Exceptions;

/// <summary>The exception that is thrown when an unsupported OFX version is encountered.</summary>
public class UnsupportedVersionException : OfxParseException
{
    /// <summary>Gets the unsupported version string.</summary>
    public string Version { get; }

    /// <summary>Initializes a new instance with the unsupported version and a message.</summary>
    public UnsupportedVersionException(string version, string message)
        : base(message)
    {
        Version = version;
    }
}
