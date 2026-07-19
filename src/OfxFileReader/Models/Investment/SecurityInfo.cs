namespace OfxFileReader.Models.Investment;

/// <summary>Represents security information (SECINFO) referenced in investment statements.</summary>
public sealed record SecurityInfo(
    /// <summary>The unique security identifier.</summary>
    string SecurityId,
    /// <summary>The type of security identifier (e.g., CUSIP).</summary>
    string SecurityIdType,
    /// <summary>The full name of the security.</summary>
    string? SecurityName = null,
    /// <summary>The ticker symbol of the security.</summary>
    string? Ticker = null,
    /// <summary>The name of the financial institution managing the security.</summary>
    string? FiName = null,
    /// <summary>The type of units (e.g., SHARES, FACE).</summary>
    string? UnitType = null,
    /// <summary>The type of security (e.g., STOCK, MUTUALFUND).</summary>
    string? SecurityType = null,
    /// <summary>The security rating, if available.</summary>
    decimal? Rating = null
);
