namespace OfxFileReader.Exceptions;

public class UnsupportedVersionException : OfxParseException
{
    public string Version { get; }

    public UnsupportedVersionException(string version, string message)
        : base(message)
    {
        Version = version;
    }
}
