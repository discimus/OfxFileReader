namespace OfxFileReader.Models.Common;

/// <summary>Represents a currency with an optional exchange rate.</summary>
/// <param name="Code">The ISO 4217 currency code (e.g., USD, EUR, BRL).</param>
/// <param name="Rate">The exchange rate relative to the default currency, if applicable.</param>
public sealed record Currency(string Code, decimal? Rate = null);
