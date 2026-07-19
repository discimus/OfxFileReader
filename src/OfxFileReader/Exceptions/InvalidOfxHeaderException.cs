namespace OfxFileReader.Exceptions;

/// <summary>The exception that is thrown when an OFX file contains invalid or malformed header information.</summary>
public class InvalidOfxHeaderException : OfxParseException
{
    /// <summary>Gets the raw header content that caused the exception, if available.</summary>
    public string? RawHeader { get; }

    /// <summary>Initializes a new instance with a message and optional raw header content.</summary>
    public InvalidOfxHeaderException(string message, string? rawHeader = null)
        : base(message)
    {
        RawHeader = rawHeader;
    }
}
