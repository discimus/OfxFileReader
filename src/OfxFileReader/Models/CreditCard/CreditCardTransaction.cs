using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.CreditCard;

/// <summary>Represents a single credit card transaction (STMTTRN) from an OFX credit card statement.</summary>
public sealed record CreditCardTransaction(
    /// <summary>The transaction type (credit, debit, etc.).</summary>
    TransactionType Type,
    /// <summary>The date the transaction was posted.</summary>
    DateTimeOffset DatePosted,
    /// <summary>The date the user initiated the transaction, if available.</summary>
    DateTimeOffset? DateUser,
    /// <summary>The date the funds become available, if available.</summary>
    DateTimeOffset? DateAvailable,
    /// <summary>The transaction amount (positive for credits, negative for debits).</summary>
    decimal Amount,
    /// <summary>The financial institution's transaction identifier.</summary>
    string FitId,
    /// <summary>The name or description of the transaction.</summary>
    string? Name = null,
    /// <summary>Additional memo or description text.</summary>
    string? Memo = null,
    /// <summary>The reference number assigned by the financial institution.</summary>
    string? ReferenceNumber = null,
    /// <summary>The payee identifier, if available.</summary>
    string? PayeeId = null,
    /// <summary>The Standard Industrial Classification code, if available.</summary>
    string? Sic = null,
    /// <summary>The currency code for this transaction, if different from default.</summary>
    string? CurrencyCode = null,
    /// <summary>The currency exchange rate, if applicable.</summary>
    decimal? CurrencyRate = null
);
