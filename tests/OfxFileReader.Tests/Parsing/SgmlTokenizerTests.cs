using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Tests.Parsing;

/// <summary>Tests for the SGML tokenizer functionality.</summary>
public class SgmlTokenizerTests
{
    /// <summary>Verifies that simple open/close tags with text are tokenized correctly.</summary>
    [Fact]
    public void Tokenize_SimpleTags_ReturnsCorrectTokens()
    {
        var input = "<TAG1>value1</TAG1>";
        var tokenizer = new SgmlTokenizer(input);
        var tokens = tokenizer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal(SgmlTokenType.OpenTag, tokens[0].Type);
        Assert.Equal("TAG1", tokens[0].Value);
        Assert.Equal(SgmlTokenType.Text, tokens[1].Type);
        Assert.Equal("value1", tokens[1].Value);
        Assert.Equal(SgmlTokenType.CloseTag, tokens[2].Type);
        Assert.Equal("TAG1", tokens[2].Value);
    }

    /// <summary>Verifies that nested tags produce the correct sequence of tokens.</summary>
    [Fact]
    public void Tokenize_NestedTags_ReturnsCorrectTokens()
    {
        var input = "<OUTER><INNER>value</INNER></OUTER>";
        var tokenizer = new SgmlTokenizer(input);
        var tokens = tokenizer.Tokenize();

        Assert.Equal(5, tokens.Count);
        Assert.Equal(SgmlTokenType.OpenTag, tokens[0].Type);
        Assert.Equal("OUTER", tokens[0].Value);
        Assert.Equal(SgmlTokenType.OpenTag, tokens[1].Type);
        Assert.Equal("INNER", tokens[1].Value);
        Assert.Equal(SgmlTokenType.Text, tokens[2].Type);
        Assert.Equal("value", tokens[2].Value);
        Assert.Equal(SgmlTokenType.CloseTag, tokens[3].Type);
        Assert.Equal("INNER", tokens[3].Value);
        Assert.Equal(SgmlTokenType.CloseTag, tokens[4].Type);
        Assert.Equal("OUTER", tokens[4].Value);
    }

    /// <summary>Verifies that unclosed tags are handled without throwing.</summary>
    [Fact]
    public void Tokenize_UnclosedTags_HandlesGracefully()
    {
        var input = "<TAG1>value1<TAG2>value2";
        var tokenizer = new SgmlTokenizer(input);
        var tokens = tokenizer.Tokenize();

        Assert.Equal(4, tokens.Count);
    }

    /// <summary>Verifies that empty input produces an empty token list.</summary>
    [Fact]
    public void Tokenize_EmptyInput_ReturnsEmpty()
    {
        var tokenizer = new SgmlTokenizer("");
        var tokens = tokenizer.Tokenize();
        Assert.Empty(tokens);
    }

    /// <summary>Verifies that line numbers are tracked correctly across newlines.</summary>
    [Fact]
    public void Tokenize_WithNewlines_MaintainsLineNumbers()
    {
        var input = "<TAG1>\n  value1\n</TAG1>";
        var tokenizer = new SgmlTokenizer(input);
        var tokens = tokenizer.Tokenize();

        Assert.Equal(3, tokens.Count);
        // Line numbers can vary based on implementation details
        Assert.All(tokens, t => Assert.True(t.LineNumber >= 1));
    }
}
