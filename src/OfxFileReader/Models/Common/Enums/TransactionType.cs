namespace OfxFileReader.Models.Common.Enums;

/// <summary>Defines the types of financial transactions that can appear in OFX statements.</summary>
public enum TransactionType
{
    /// <summary>Unknown or unidentifiable transaction type.</summary>
    Unknown = 0,
    /// <summary>A generic credit (money in).</summary>
    Credit,
    /// <summary>A generic debit (money out).</summary>
    Debit,
    /// <summary>Interest earned or charged.</summary>
    Interest,
    /// <summary>Dividend payment.</summary>
    Dividend,
    /// <summary>Service fee charged.</summary>
    Fee,
    /// <summary>Service charge assessed by the institution.</summary>
    ServiceCharge,
    /// <summary>A deposit transaction.</summary>
    Deposit,
    /// <summary>An ATM transaction.</summary>
    ATM,
    /// <summary>A point-of-sale transaction.</summary>
    PointOfSale,
    /// <summary>A funds transfer between accounts.</summary>
    Transfer,
    /// <summary>A check transaction.</summary>
    Check,
    /// <summary>A payment transaction.</summary>
    Payment,
    /// <summary>A cash withdrawal or deposit.</summary>
    Cash,
    /// <summary>A direct deposit transaction.</summary>
    DirectDeposit,
    /// <summary>A direct debit transaction.</summary>
    DirectDebit,
    /// <summary>A recurring or repeat payment.</summary>
    RepeatPayment,
    /// <summary>Any other transaction type not covered by the standard codes.</summary>
    Other
}
