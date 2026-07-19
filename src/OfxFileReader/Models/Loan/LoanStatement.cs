using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.Loan;

/// <summary>Represents a loan statement (LOANSTMTRS) from an OFX response.</summary>
public sealed record LoanStatement(
    /// <summary>The default currency code for the statement.</summary>
    string? Currency,
    /// <summary>The loan account information.</summary>
    LoanAccount Account,
    /// <summary>The remaining principal balance.</summary>
    Balance? PrincipalBalance,
    /// <summary>The current ledger balance.</summary>
    Balance? LedgerBalance,
    /// <summary>The available balance.</summary>
    Balance? AvailableBalance,
    /// <summary>The start date of the statement period.</summary>
    DateTimeOffset StartDate,
    /// <summary>The end date of the statement period.</summary>
    DateTimeOffset EndDate,
    /// <summary>The current interest rate as a percentage.</summary>
    decimal? InterestRate,
    /// <summary>The year-to-date interest paid.</summary>
    decimal? InterestYearToDate,
    /// <summary>The date of the next payment due.</summary>
    DateTimeOffset? NextPaymentDate,
    /// <summary>The amount of the next payment due.</summary>
    decimal? NextPaymentAmount,
    /// <summary>Pay-to address information (complex, currently skipped).</summary>
    decimal? PayToAddress,
    /// <summary>The list of loan transactions in the statement period.</summary>
    IReadOnlyList<LoanTransaction>? Transactions,
    /// <summary>Optional transaction UID from the response wrapper.</summary>
    string? TransactionUid = null
);
