namespace OfxFileReader.Exceptions;

public class MissingRequiredFieldException : OfxParseException
{
    public string FieldName { get; }

    public MissingRequiredFieldException(string fieldName, string tagName)
        : base($"Required field '{fieldName}' is missing in tag '{tagName}'", tagName: tagName)
    {
        FieldName = fieldName;
    }
}
