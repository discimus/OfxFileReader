using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.Loan;

public sealed record LoanStatement(
    string? Currency,
    LoanAccount Account,
    Balance? PrincipalBalance,
    Balance? LedgerBalance,
    Balance? AvailableBalance,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    decimal? InterestRate,
    decimal? InterestYearToDate,
    DateTimeOffset? NextPaymentDate,
    decimal? NextPaymentAmount,
    decimal? PayToAddress,
    IReadOnlyList<LoanTransaction>? Transactions,
    string? TransactionUid = null
);
