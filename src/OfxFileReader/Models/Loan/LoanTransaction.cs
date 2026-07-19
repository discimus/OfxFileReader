using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Loan;

public sealed record LoanTransaction(
    TransactionType Type,
    DateTimeOffset DatePosted,
    decimal Amount,
    string FitId,
    string? Name = null,
    string? Memo = null,
    decimal? PrincipalAmount = null,
    decimal? InterestAmount = null,
    decimal? EscrowAmount = null,
    string? CurrencyCode = null,
    decimal? CurrencyRate = null
);
