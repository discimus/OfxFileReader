using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

/// <summary>Comprehensive edge case tests for the transaction type converter.</summary>
public class TransactionTypeConverterComprehensiveTests
{
    [Theory]
    [InlineData("credit", TransactionType.Credit)]
    [InlineData("Credit", TransactionType.Credit)]
    [InlineData(" CREDIT ", TransactionType.Credit)]
    [InlineData("debit", TransactionType.Debit)]
    [InlineData("Debit", TransactionType.Debit)]
    /// <summary>Verifies that the converter is case-insensitive and trims whitespace.</summary>
    public void Parse_CaseInsensitive_ReturnsCorrectEnum(string input, TransactionType expected)
    {
        var result = TransactionTypeConverter.Parse(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    /// <summary>Verifies that null, empty, or whitespace input returns Unknown.</summary>
    public void Parse_NullEmptyWhitespace_ReturnsUnknown(string? input)
    {
        var result = TransactionTypeConverter.Parse(input);
        Assert.Equal(TransactionType.Unknown, result);
    }
}
