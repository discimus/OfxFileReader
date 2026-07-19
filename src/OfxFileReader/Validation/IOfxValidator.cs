using OfxFileReader.Models;

namespace OfxFileReader.Validation;

/// <summary>Defines the contract for validating parsed OFX documents.</summary>
public interface IOfxValidator
{
    /// <summary>Validates the OFX document and returns a list of validation error messages.</summary>
    IReadOnlyList<string> Validate(OfxDocument document);

    /// <summary>Returns whether the OFX document is valid (no validation errors).</summary>
    bool IsValid(OfxDocument document);
}
