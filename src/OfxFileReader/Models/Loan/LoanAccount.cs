using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Loan;

/// <summary>Represents a loan account (LOANACCTFROM) from an OFX statement.</summary>
public sealed record LoanAccount(
    /// <summary>The bank routing or transit number.</summary>
    string BankId,
    /// <summary>The loan account identifier.</summary>
    string AccountId,
    /// <summary>The type of loan (e.g., Loan, LineOfCredit).</summary>
    AccountType LoanType
);
