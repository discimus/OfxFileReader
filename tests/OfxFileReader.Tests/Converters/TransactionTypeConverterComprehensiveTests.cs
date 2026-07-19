using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Converters;

namespace OfxFileReader.Tests.Converters;

public class TransactionTypeConverterComprehensiveTests
{
    [Theory]
    [InlineData("credit", TransactionType.Credit)]
    [InlineData("Credit", TransactionType.Credit)]
    [InlineData(" CREDIT ", TransactionType.Credit)]
    [InlineData("debit", TransactionType.Debit)]
    [InlineData("Debit", TransactionType.Debit)]
    public void Parse_CaseInsensitive_ReturnsCorrectEnum(string input, TransactionType expected)
    {
        var result = TransactionTypeConverter.Parse(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_NullEmptyWhitespace_ReturnsUnknown(string? input)
    {
        var result = TransactionTypeConverter.Parse(input);
        Assert.Equal(TransactionType.Unknown, result);
    }
}
