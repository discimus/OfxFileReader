namespace OfxFileReader.Models.Common;

/// <summary>Represents a balance with an amount and effective date.</summary>
/// <param name="Amount">The balance amount.</param>
/// <param name="AsOfDate">The date and time as of which the balance is effective.</param>
public sealed record Balance(decimal Amount, DateTimeOffset AsOfDate);
