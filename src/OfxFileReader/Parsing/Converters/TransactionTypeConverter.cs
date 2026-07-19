using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Parsing.Converters;

internal static class TransactionTypeConverter
{
    private static readonly Dictionary<string, TransactionType> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CREDIT"] = TransactionType.Credit,
        ["DEBIT"] = TransactionType.Debit,
        ["INT"] = TransactionType.Interest,
        ["DIV"] = TransactionType.Dividend,
        ["FEE"] = TransactionType.Fee,
        ["SRVCHG"] = TransactionType.ServiceCharge,
        ["DEP"] = TransactionType.Deposit,
        ["ATM"] = TransactionType.ATM,
        ["POS"] = TransactionType.PointOfSale,
        ["XFER"] = TransactionType.Transfer,
        ["CHECK"] = TransactionType.Check,
        ["PAYMENT"] = TransactionType.Payment,
        ["CASH"] = TransactionType.Cash,
        ["DIRECTDEP"] = TransactionType.DirectDeposit,
        ["DIRECTDEBIT"] = TransactionType.DirectDebit,
        ["REPEATPMT"] = TransactionType.RepeatPayment,
        ["OTHER"] = TransactionType.Other,
    };

    public static TransactionType Parse(string? value)
    {
        if (value is not null && Map.TryGetValue(value.Trim(), out var type))
            return type;

        return TransactionType.Unknown;
    }
}
