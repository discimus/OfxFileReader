using OfxFileReader.Models;

namespace OfxFileReader.Validation;

public interface IOfxValidator
{
    IReadOnlyList<string> Validate(OfxDocument document);
    bool IsValid(OfxDocument document);
}
