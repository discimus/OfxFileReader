using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

/// <summary>Tests for the investment transaction type converter.</summary>
public class InvestmentTransactionTypeConverterTests
{
    [Theory]
    [InlineData("BUY", InvestmentTransactionType.Buy)]
    [InlineData("BUYSTOCK", InvestmentTransactionType.Buy)]
    [InlineData("BUYMF", InvestmentTransactionType.Buy)]
    [InlineData("BUYOPT", InvestmentTransactionType.Buy)]
    [InlineData("BUYOTHER", InvestmentTransactionType.Buy)]
    [InlineData("SELL", InvestmentTransactionType.Sell)]
    [InlineData("SELLSTOCK", InvestmentTransactionType.Sell)]
    [InlineData("SELLMF", InvestmentTransactionType.Sell)]
    [InlineData("SELLOPT", InvestmentTransactionType.Sell)]
    [InlineData("SELLOTHER", InvestmentTransactionType.Sell)]
    [InlineData("REINVEST", InvestmentTransactionType.ReinvestDividend)]
    [InlineData("RETOFCAP", InvestmentTransactionType.ReturnOfCapital)]
    [InlineData("TRANSFER", InvestmentTransactionType.Transfer)]
    [InlineData("SPLIT", InvestmentTransactionType.Split)]
    [InlineData("INT", InvestmentTransactionType.Interest)]
    [InlineData("DIV", InvestmentTransactionType.Dividend)]
    [InlineData("MARGININT", InvestmentTransactionType.MarginInterest)]
    [InlineData("INCOME", InvestmentTransactionType.MiscIncome)]
    [InlineData("EXPENSE", InvestmentTransactionType.MiscExpense)]
    [InlineData("MISCEXPENSE", InvestmentTransactionType.MiscExpense)]
    [InlineData("MISCINCOME", InvestmentTransactionType.MiscIncome)]
    [InlineData("buystock", InvestmentTransactionType.Buy)]
    [InlineData("BuyStock", InvestmentTransactionType.Buy)]
    /// <summary>Verifies that all known investment transaction types map correctly.</summary>
    public void Parse_AllKnownTypes_ReturnsCorrectEnum(string input, InvestmentTransactionType expected)
    {
        var result = InvestmentTransactionTypeConverter.Parse(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("NONSENSE")]
    /// <summary>Verifies that invalid input returns Unknown.</summary>
    public void Parse_InvalidInput_ReturnsUnknown(string? input)
    {
        var result = InvestmentTransactionTypeConverter.Parse(input);
        Assert.Equal(InvestmentTransactionType.Unknown, result);
    }
}
