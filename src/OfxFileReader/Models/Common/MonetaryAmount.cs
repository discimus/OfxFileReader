namespace OfxFileReader.Models.Common;

/// <summary>Represents a monetary amount with its associated currency.</summary>
/// <param name="Amount">The monetary value.</param>
/// <param name="Currency">The ISO 4217 currency code.</param>
public sealed record MonetaryAmount(decimal Amount, string Currency);
