namespace OfxFileReader.Models.Common.Enums;

public enum TransactionType
{
    Unknown = 0,
    Credit,
    Debit,
    Interest,
    Dividend,
    Fee,
    ServiceCharge,
    Deposit,
    ATM,
    PointOfSale,
    Transfer,
    Check,
    Payment,
    Cash,
    DirectDeposit,
    DirectDebit,
    RepeatPayment,
    Other
}
