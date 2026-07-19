using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Banking;

/// <summary>Represents a bank account (BANKACCTFROM) from an OFX statement.</summary>
public sealed record BankAccount(
    /// <summary>The bank routing or transit number.</summary>
    string BankId,
    /// <summary>The account identifier number.</summary>
    string AccountId,
    /// <summary>The type of bank account (checking, savings, etc.).</summary>
    AccountType AccountType,
    /// <summary>The branch identifier, if available.</summary>
    string? BranchId = null,
    /// <summary>The account key for additional identification, if available.</summary>
    string? AccountKey = null
);
