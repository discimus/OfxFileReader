namespace OfxFileReader.Models.Common.Enums;

/// <summary>Defines the types of investment transactions in OFX statements.</summary>
public enum InvestmentTransactionType
{
    /// <summary>Unknown or unidentifiable investment transaction type.</summary>
    Unknown = 0,
    /// <summary>Buy or purchase of a security.</summary>
    Buy,
    /// <summary>Sell of a security.</summary>
    Sell,
    /// <summary>Reinvestment of dividends.</summary>
    ReinvestDividend,
    /// <summary>Return of capital distribution.</summary>
    ReturnOfCapital,
    /// <summary>Transfer of assets in or out.</summary>
    Transfer,
    /// <summary>Stock split.</summary>
    Split,
    /// <summary>Interest income or expense.</summary>
    Interest,
    /// <summary>Dividend income.</summary>
    Dividend,
    /// <summary>Margin interest charged.</summary>
    MarginInterest,
    /// <summary>Miscellaneous expense.</summary>
    MiscExpense,
    /// <summary>Miscellaneous income.</summary>
    MiscIncome
}
