namespace OfxFileReader.Exceptions;

/// <summary>The exception that is thrown when a required OFX field is missing during parsing.</summary>
public class MissingRequiredFieldException : OfxParseException
{
    /// <summary>Gets the name of the missing field.</summary>
    public string FieldName { get; }

    /// <summary>Initializes a new instance with the missing field name and containing tag.</summary>
    public MissingRequiredFieldException(string fieldName, string tagName)
        : base($"Required field '{fieldName}' is missing in tag '{tagName}'", tagName: tagName)
    {
        FieldName = fieldName;
    }
}
