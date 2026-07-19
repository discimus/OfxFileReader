using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Investment;

/// <summary>Represents an investment transaction within an investment statement.</summary>
public sealed record InvestmentTransaction(
    /// <summary>The financial institution's transaction identifier.</summary>
    string FitId,
    /// <summary>The date the transaction was posted.</summary>
    DateTimeOffset DatePosted,
    /// <summary>The settlement date, if available.</summary>
    DateTimeOffset? DateSettle,
    /// <summary>The type of investment transaction (buy, sell, etc.).</summary>
    InvestmentTransactionType Type,
    /// <summary>The number of units or shares transacted.</summary>
    decimal? Units = null,
    /// <summary>The price per unit.</summary>
    decimal? UnitPrice = null,
    /// <summary>The total transaction amount.</summary>
    decimal? TotalAmount = null,
    /// <summary>The commission charged, if any.</summary>
    decimal? Commission = null,
    /// <summary>The fees charged, if any.</summary>
    decimal? Fees = null,
    /// <summary>The load (sales charge) amount, if any.</summary>
    decimal? Load = null,
    /// <summary>The interest amount, if applicable.</summary>
    decimal? Interest = null,
    /// <summary>The realized gain or loss, if applicable.</summary>
    decimal? Gain = null,
    /// <summary>The security identifier, if available.</summary>
    string? SecurityId = null,
    /// <summary>The type of security identifier (e.g., CUSIP).</summary>
    string? SecurityIdType = null,
    /// <summary>Additional memo or notes.</summary>
    string? Memo = null,
    /// <summary>The currency code for this transaction, if different from default.</summary>
    string? CurrencyCode = null,
    /// <summary>The currency exchange rate, if applicable.</summary>
    decimal? CurrencyRate = null,
    /// <summary>The 401(k) source, if applicable (e.g., PRETAX, EMPLOYER).</summary>
    string? Inv401kSource = null
);
