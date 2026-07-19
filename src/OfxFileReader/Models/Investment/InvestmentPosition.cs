namespace OfxFileReader.Models.Investment;

/// <summary>Represents an investment position (INVPOS) held in an account.</summary>
public sealed record InvestmentPosition(
    /// <summary>The security identifier.</summary>
    string SecurityId,
    /// <summary>The type of security identifier (e.g., CUSIP).</summary>
    string SecurityIdType,
    /// <summary>How the position is held (e.g., CASH, MARGIN).</summary>
    string HeldInAccount,
    /// <summary>The position type (e.g., LONG, SHORT).</summary>
    string PositionType,
    /// <summary>The number of units or shares held.</summary>
    decimal Units,
    /// <summary>The price per unit at the statement date.</summary>
    decimal UnitPrice,
    /// <summary>The total market value of the position.</summary>
    decimal MarketValue,
    /// <summary>The date as of which the price is effective.</summary>
    DateTimeOffset PriceAsOfDate,
    /// <summary>The currency code for this position, if different from default.</summary>
    string? CurrencyCode = null,
    /// <summary>The currency exchange rate, if applicable.</summary>
    decimal? CurrencyRate = null,
    /// <summary>Additional memo or notes.</summary>
    string? Memo = null
);
