namespace OfxFileReader.Models.Investment;

/// <summary>Represents an investment account (INVACCTFROM) from an OFX statement.</summary>
/// <param name="BrokerId">The broker or financial institution identifier.</param>
/// <param name="AccountId">The investment account number.</param>
public sealed record InvestmentAccount(string BrokerId, string AccountId);
