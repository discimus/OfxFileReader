using OfxFileReader.Models;
using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Validation;

/// <summary>Validates parsed OFX documents for required fields and data integrity.</summary>
public sealed class OfxValidator : IOfxValidator
{
    /// <summary>Validates the OFX document and returns a list of validation error messages.</summary>
    public IReadOnlyList<string> Validate(OfxDocument document)
    {
        var errors = new List<string>();

        if (document.Header is null)
        {
            errors.Add("Document header is missing");
            return errors.AsReadOnly();
        }

        if (document.Header.Version == OfxVersion.Unknown)
            errors.Add("Unknown OFX version");

        if (document.Header.OfxHeaderValue <= 0)
            errors.Add("Invalid or missing OFXHEADER value");

        if (document.SignOn is null)
            errors.Add("SIGNONMSGSRSV1/SONRS is missing");
        else
        {
            if (document.SignOn.ServerDate == DateTimeOffset.MinValue)
                errors.Add("SONRS.DTSERVER is missing or invalid");
        }

        // Validate bank statements
        if (document.BankStatements is not null)
        {
            foreach (var stmt in document.BankStatements)
            {
                if (string.IsNullOrWhiteSpace(stmt.Account?.AccountId))
                    errors.Add("Bank statement missing account ID");
                if (stmt.Transactions.Count == 0)
                    errors.Add($"Bank statement {stmt.Account?.AccountId} has no transactions");
            }
        }

        // Validate credit card statements
        if (document.CreditCardStatements is not null)
        {
            foreach (var stmt in document.CreditCardStatements)
            {
                if (string.IsNullOrWhiteSpace(stmt.Account?.AccountId))
                    errors.Add("Credit card statement missing account ID");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>Returns whether the OFX document is valid.</summary>
    public bool IsValid(OfxDocument document)
    {
        return Validate(document).Count == 0;
    }
}
