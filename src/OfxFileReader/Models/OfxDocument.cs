using OfxFileReader.Models.Banking;
using OfxFileReader.Models.CreditCard;
using OfxFileReader.Models.Investment;
using OfxFileReader.Models.Loan;
using OfxFileReader.Models.SignOn;

namespace OfxFileReader.Models;

public sealed record OfxDocument(
    OfxHeader Header,
    OfxParseMetadata Metadata,
    SignOnResponse? SignOn = null,
    IReadOnlyList<BankStatement>? BankStatements = null,
    IReadOnlyList<CreditCardStatement>? CreditCardStatements = null,
    IReadOnlyList<InvestmentStatement>? InvestmentStatements = null,
    IReadOnlyList<LoanStatement>? LoanStatements = null
);
