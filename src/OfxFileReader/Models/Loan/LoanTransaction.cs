using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Loan;

/// <summary>Represents a single loan transaction (LOANTRN) from an OFX loan statement.</summary>
public sealed record LoanTransaction(
    /// <summary>The transaction type (debit, credit, etc.).</summary>
    TransactionType Type,
    /// <summary>The date the transaction was posted.</summary>
    DateTimeOffset DatePosted,
    /// <summary>The total transaction amount.</summary>
    decimal Amount,
    /// <summary>The financial institution's transaction identifier.</summary>
    string FitId,
    /// <summary>The name or description of the transaction.</summary>
    string? Name = null,
    /// <summary>Additional memo or description text.</summary>
    string? Memo = null,
    /// <summary>The portion of the payment applied to principal.</summary>
    decimal? PrincipalAmount = null,
    /// <summary>The portion of the payment applied to interest.</summary>
    decimal? InterestAmount = null,
    /// <summary>The escrow amount included in the payment, if any.</summary>
    decimal? EscrowAmount = null,
    /// <summary>The currency code for this transaction, if different from default.</summary>
    string? CurrencyCode = null,
    /// <summary>The currency exchange rate, if applicable.</summary>
    decimal? CurrencyRate = null
);
