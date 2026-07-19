namespace OfxFileReader.Models.CreditCard;

/// <summary>Represents a credit card account (CCACCTFROM) from an OFX statement.</summary>
/// <param name="AccountId">The credit card account number.</param>
public sealed record CreditCardAccount(string AccountId);
