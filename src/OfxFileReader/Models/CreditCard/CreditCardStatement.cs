using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.CreditCard;

public sealed record CreditCardStatement(
    string? Currency,
    CreditCardAccount Account,
    Balance? LedgerBalance,
    Balance? AvailableBalance,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    IReadOnlyList<CreditCardTransaction> Transactions,
    string? MarketingInfo = null,
    string? TransactionUid = null
);
