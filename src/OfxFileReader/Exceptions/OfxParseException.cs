namespace OfxFileReader.Exceptions;

/// <summary>The base exception thrown when an OFX parsing error occurs.</summary>
public class OfxParseException : Exception
{
    /// <summary>Gets the line number where the parsing error occurred, if available.</summary>
    public int? LineNumber { get; }

    /// <summary>Gets the tag name where the parsing error occurred, if available.</summary>
    public string? TagName { get; }

    /// <summary>Initializes a new instance with a message and optional line/tag information.</summary>
    public OfxParseException(string message, int? lineNumber = null, string? tagName = null)
        : base(message)
    {
        LineNumber = lineNumber;
        TagName = tagName;
    }

    /// <summary>Initializes a new instance with a message, inner exception, and optional line/tag information.</summary>
    public OfxParseException(string message, Exception innerException, int? lineNumber = null, string? tagName = null)
        : base(message, innerException)
    {
        LineNumber = lineNumber;
        TagName = tagName;
    }
}
