using OfxFileReader.Models.Banking;
using OfxFileReader.Models.CreditCard;
using OfxFileReader.Models.Investment;
using OfxFileReader.Models.Loan;
using OfxFileReader.Models.SignOn;

namespace OfxFileReader.Models;

/// <summary>Represents the complete parsed OFX document with all financial data.</summary>
public sealed record OfxDocument(
    /// <summary>The parsed OFX header information.</summary>
    OfxHeader Header,
    /// <summary>Metadata about the parsing process.</summary>
    OfxParseMetadata Metadata,
    /// <summary>The sign-on response, or null if not present.</summary>
    SignOnResponse? SignOn = null,
    /// <summary>The list of bank statements, or null if not present.</summary>
    IReadOnlyList<BankStatement>? BankStatements = null,
    /// <summary>The list of credit card statements, or null if not present.</summary>
    IReadOnlyList<CreditCardStatement>? CreditCardStatements = null,
    /// <summary>The list of investment statements, or null if not present.</summary>
    IReadOnlyList<InvestmentStatement>? InvestmentStatements = null,
    /// <summary>The list of loan statements, or null if not present.</summary>
    IReadOnlyList<LoanStatement>? LoanStatements = null
);
