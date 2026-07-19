using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.Investment;

/// <summary>Represents an investment statement (INVSTMTRS) from an OFX response.</summary>
public sealed record InvestmentStatement(
    /// <summary>The date and time as of which the statement data is current.</summary>
    DateTimeOffset AsOfDate,
    /// <summary>The default currency code for the statement (e.g., USD).</summary>
    string? Currency,
    /// <summary>The investment account information.</summary>
    InvestmentAccount Account,
    /// <summary>The available cash balance in the account.</summary>
    Balance? AvailableCash,
    /// <summary>The margin balance, if applicable.</summary>
    decimal? MarginBalance,
    /// <summary>The short balance, if applicable.</summary>
    decimal? ShortBalance,
    /// <summary>The start date of the statement period.</summary>
    DateTimeOffset? StartDate,
    /// <summary>The end date of the statement period.</summary>
    DateTimeOffset? EndDate,
    /// <summary>The list of investment positions held in the account.</summary>
    IReadOnlyList<InvestmentPosition>? Positions,
    /// <summary>The list of investment transactions in the statement period.</summary>
    IReadOnlyList<InvestmentTransaction>? Transactions,
    /// <summary>The list of security information referenced in the statement.</summary>
    IReadOnlyList<SecurityInfo>? Securities,
    /// <summary>Optional transaction UID from the response wrapper.</summary>
    string? TransactionUid = null
);
