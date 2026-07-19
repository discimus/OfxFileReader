using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.Banking;

/// <summary>Represents a bank statement (STMTRS) from an OFX response.</summary>
public sealed record BankStatement(
    /// <summary>The default currency code for the statement (e.g., USD, BRL).</summary>
    string? Currency,
    /// <summary>The bank account information.</summary>
    BankAccount Account,
    /// <summary>The ledger balance at the end of the statement period.</summary>
    Balance? LedgerBalance,
    /// <summary>The available balance at the end of the statement period.</summary>
    Balance? AvailableBalance,
    /// <summary>The start date of the statement period.</summary>
    DateTimeOffset StartDate,
    /// <summary>The end date of the statement period.</summary>
    DateTimeOffset EndDate,
    /// <summary>The list of transactions in this statement.</summary>
    IReadOnlyList<BankTransaction> Transactions,
    /// <summary>Optional marketing information text.</summary>
    string? MarketingInfo = null,
    /// <summary>Optional transaction UID from the response wrapper.</summary>
    string? TransactionUid = null
);
