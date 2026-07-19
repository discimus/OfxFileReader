using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

/// <summary>Tests for the transaction type converter.</summary>
public class TransactionTypeConverterTests
{
    [Theory]
    [InlineData("CREDIT", TransactionType.Credit)]
    [InlineData("DEBIT", TransactionType.Debit)]
    [InlineData("INT", TransactionType.Interest)]
    [InlineData("DIV", TransactionType.Dividend)]
    [InlineData("FEE", TransactionType.Fee)]
    [InlineData("SRVCHG", TransactionType.ServiceCharge)]
    [InlineData("DEP", TransactionType.Deposit)]
    [InlineData("ATM", TransactionType.ATM)]
    [InlineData("POS", TransactionType.PointOfSale)]
    [InlineData("XFER", TransactionType.Transfer)]
    [InlineData("CHECK", TransactionType.Check)]
    [InlineData("PAYMENT", TransactionType.Payment)]
    [InlineData("CASH", TransactionType.Cash)]
    [InlineData("DIRECTDEP", TransactionType.DirectDeposit)]
    [InlineData("DIRECTDEBIT", TransactionType.DirectDebit)]
    [InlineData("REPEATPMT", TransactionType.RepeatPayment)]
    [InlineData("OTHER", TransactionType.Other)]
    /// <summary>Verifies that all known transaction type strings map to the correct enum values.</summary>
    public void Parse_AllKnownTypes_ReturnsCorrectEnum(string input, TransactionType expected)
    {
        var result = TransactionTypeConverter.Parse(input);
        Assert.Equal(expected, result);
    }

    /// <summary>Verifies that an unknown transaction type returns Unknown.</summary>
    [Fact]
    public void Parse_UnknownType_ReturnsUnknown()
    {
        var result = TransactionTypeConverter.Parse("INVALID_TYPE");
        Assert.Equal(TransactionType.Unknown, result);
    }
}
