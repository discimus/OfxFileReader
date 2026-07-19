using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.CreditCard;

/// <summary>Represents a credit card statement (CCSTMTRS) from an OFX response.</summary>
public sealed record CreditCardStatement(
    /// <summary>The default currency code for the statement (e.g., USD, BRL).</summary>
    string? Currency,
    /// <summary>The credit card account information.</summary>
    CreditCardAccount Account,
    /// <summary>The ledger balance at the end of the statement period.</summary>
    Balance? LedgerBalance,
    /// <summary>The available balance at the end of the statement period.</summary>
    Balance? AvailableBalance,
    /// <summary>The start date of the statement period.</summary>
    DateTimeOffset StartDate,
    /// <summary>The end date of the statement period.</summary>
    DateTimeOffset EndDate,
    /// <summary>The list of transactions in this statement.</summary>
    IReadOnlyList<CreditCardTransaction> Transactions,
    /// <summary>Optional marketing information text.</summary>
    string? MarketingInfo = null,
    /// <summary>Optional transaction UID from the response wrapper.</summary>
    string? TransactionUid = null
);
