using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Banking;

public sealed record BankAccount(
    string BankId,
    string AccountId,
    AccountType AccountType,
    string? BranchId = null,
    string? AccountKey = null
);
