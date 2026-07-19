using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Loan;

public sealed record LoanAccount(
    string BankId,
    string AccountId,
    AccountType LoanType
);
