using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Parsing.Converters;

/// <summary>Converts OFX investment transaction type strings to <see cref="InvestmentTransactionType"/> enum values.</summary>
internal static class InvestmentTransactionTypeConverter
{
    private static readonly Dictionary<string, InvestmentTransactionType> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BUY"] = InvestmentTransactionType.Buy,
        ["BUYSTOCK"] = InvestmentTransactionType.Buy,
        ["BUYMF"] = InvestmentTransactionType.Buy,
        ["BUYOPT"] = InvestmentTransactionType.Buy,
        ["BUYOTHER"] = InvestmentTransactionType.Buy,
        ["SELL"] = InvestmentTransactionType.Sell,
        ["SELLSTOCK"] = InvestmentTransactionType.Sell,
        ["SELLMF"] = InvestmentTransactionType.Sell,
        ["SELLOPT"] = InvestmentTransactionType.Sell,
        ["SELLOTHER"] = InvestmentTransactionType.Sell,
        ["REINVEST"] = InvestmentTransactionType.ReinvestDividend,
        ["RETOFCAP"] = InvestmentTransactionType.ReturnOfCapital,
        ["TRANSFER"] = InvestmentTransactionType.Transfer,
        ["SPLIT"] = InvestmentTransactionType.Split,
        ["INT"] = InvestmentTransactionType.Interest,
        ["DIV"] = InvestmentTransactionType.Dividend,
        ["MARGININT"] = InvestmentTransactionType.MarginInterest,
        ["INCOME"] = InvestmentTransactionType.MiscIncome,
        ["EXPENSE"] = InvestmentTransactionType.MiscExpense,
        ["MISCEXPENSE"] = InvestmentTransactionType.MiscExpense,
        ["MISCINCOME"] = InvestmentTransactionType.MiscIncome,
    };

    /// <summary>Parses an OFX investment transaction type string into an <see cref="InvestmentTransactionType"/>.</summary>
    public static InvestmentTransactionType Parse(string? value)
    {
        if (value is not null && Map.TryGetValue(value.Trim(), out var type))
            return type;

        return InvestmentTransactionType.Unknown;
    }
}
