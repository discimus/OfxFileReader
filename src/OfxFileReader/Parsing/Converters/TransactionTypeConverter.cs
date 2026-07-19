using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Parsing.Converters;

/// <summary>Converts OFX transaction type strings to <see cref="TransactionType"/> enum values.</summary>
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

    /// <summary>Parses an OFX transaction type string into a <see cref="TransactionType"/>.</summary>
    public static TransactionType Parse(string? value)
    {
        if (value is not null && Map.TryGetValue(value.Trim(), out var type))
            return type;

        return TransactionType.Unknown;
    }
}
