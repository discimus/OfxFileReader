namespace OfxFileReader.Exceptions;

public class OfxParseException : Exception
{
    public int? LineNumber { get; }
    public string? TagName { get; }

    public OfxParseException(string message, int? lineNumber = null, string? tagName = null)
        : base(message)
    {
        LineNumber = lineNumber;
        TagName = tagName;
    }

    public OfxParseException(string message, Exception innerException, int? lineNumber = null, string? tagName = null)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
        TagName = tagName;
    }
}
