namespace OfxFileReader.Exceptions;

public class InvalidOfxHeaderException : OfxParseException
{
    public string? RawHeader { get; }

    public InvalidOfxHeaderException(string message, string? rawHeader = null)
        : base(message)
    {
        RawHeader = rawHeader;
    }
}
