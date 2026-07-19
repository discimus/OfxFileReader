using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.Investment;

public sealed record InvestmentStatement(
    DateTimeOffset AsOfDate,
    string? Currency,
    InvestmentAccount Account,
    Balance? AvailableCash,
    decimal? MarginBalance,
    decimal? ShortBalance,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    IReadOnlyList<InvestmentPosition>? Positions,
    IReadOnlyList<InvestmentTransaction>? Transactions,
    IReadOnlyList<SecurityInfo>? Securities,
    string? TransactionUid = null
);
