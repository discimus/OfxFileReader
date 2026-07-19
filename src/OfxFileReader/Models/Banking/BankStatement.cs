using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.Banking;

public sealed record BankStatement(
    string? Currency,
    BankAccount Account,
    Balance? LedgerBalance,
    Balance? AvailableBalance,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    IReadOnlyList<BankTransaction> Transactions,
    string? MarketingInfo = null,
    string? TransactionUid = null
);
