namespace OfxFileReader.Models.Common.Enums;

/// <summary>Defines the types of financial accounts supported by OFX.</summary>
public enum AccountType
{
    /// <summary>Unknown or unidentifiable account type.</summary>
    Unknown = 0,
    /// <summary>A checking account.</summary>
    Checking,
    /// <summary>A savings account.</summary>
    Savings,
    /// <summary>A money market account.</summary>
    MoneyMarket,
    /// <summary>A credit card account.</summary>
    CreditCard,
    /// <summary>A line of credit account.</summary>
    LineOfCredit,
    /// <summary>An investment account.</summary>
    Investment,
    /// <summary>A loan account.</summary>
    Loan
}
