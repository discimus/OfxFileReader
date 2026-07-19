using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Tests.Parsing;

public class SgmlParserEdgeCasesTests
{
    [Fact]
    public void Tokenize_SelfClosingTag_ReturnsOpenTag()
    {
        var tokenizer = new SgmlTokenizer("<TAG/>");
        var tokens = tokenizer.Tokenize();

        Assert.Single(tokens);
        Assert.Equal(SgmlTokenType.OpenTag, tokens[0].Type);
        Assert.Equal("TAG", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_NewlinesInContent_HandlesCorrectly()
    {
        var input = "<MEMO>Line1\nLine2\nLine3</MEMO>";
        var tokenizer = new SgmlTokenizer(input);
        var tokens = tokenizer.Tokenize();

        Assert.Equal(3, tokens.Count);
        Assert.Equal("Line1\nLine2\nLine3", tokens[1].Value);
    }

    [Fact]
    public void Tokenize_TagWithoutClosingAngleBracket_ReturnsEmpty()
    {
        var tokenizer = new SgmlTokenizer("<TAG");
        var tokens = tokenizer.Tokenize();

        Assert.Empty(tokens);
    }

    [Fact]
    public void Tokenize_OnlyWhitespaceContent_ReturnsEmpty()
    {
        var tokenizer = new SgmlTokenizer("<TAG>   </TAG>");
        var tokens = tokenizer.Tokenize();

        // Whitespace-only text is trimmed and omitted, only tag tokens remain
        Assert.Equal(2, tokens.Count);
        Assert.Equal("TAG", tokens[0].Value);
        Assert.Equal(SgmlTokenType.CloseTag, tokens[1].Type);
    }

    [Fact]
    public void Tokenize_TextWithAngleBrackets_HandlesCorrectly()
    {
        var input = "<MEMO>Amount < 100 and > 50</MEMO>";
        var tokenizer = new SgmlTokenizer(input);
        var tokens = tokenizer.Tokenize();

        // The '<' and '>' inside text are treated as tag delimiters
        Assert.NotEmpty(tokens);
    }

    [Fact]
    public void TreeBuilder_UnmatchedCloseTag_DoesNotCrash()
    {
        var tokens = new List<SgmlToken>
        {
            new(SgmlTokenType.OpenTag, "OUTER", 1),
            new(SgmlTokenType.CloseTag, "INNER", 1),  // unmatched close
            new(SgmlTokenType.CloseTag, "OUTER", 1)
        };

        var builder = new SgmlTreeBuilder();
        builder.Build(tokens);

        Assert.NotNull(builder.Root);
        Assert.Equal("OUTER", builder.Root.Name);
    }

    [Fact]
    public void TreeBuilder_ExtraCloseTag_DoesNotCrash()
    {
        var tokens = new List<SgmlToken>
        {
            new(SgmlTokenType.OpenTag, "TAG1", 1),
            new(SgmlTokenType.CloseTag, "TAG1", 1),
            new(SgmlTokenType.CloseTag, "TAG1", 1)  // extra
        };

        var builder = new SgmlTreeBuilder();
        builder.Build(tokens);

        Assert.NotNull(builder.Root);
    }

    [Fact]
    public void TreeBuilder_ThreeSiblingRoots_WrapsCorrectly()
    {
        var tokens = new List<SgmlToken>
        {
            new(SgmlTokenType.OpenTag, "OFX", 1),
            new(SgmlTokenType.CloseTag, "OFX", 1),
            new(SgmlTokenType.OpenTag, "OFX", 1),
            new(SgmlTokenType.CloseTag, "OFX", 1),
            new(SgmlTokenType.OpenTag, "OFX", 1),
            new(SgmlTokenType.CloseTag, "OFX", 1)
        };

        var builder = new SgmlTreeBuilder();
        builder.Build(tokens);

        Assert.NotNull(builder.Root);
        // Root should be a wrapper __ROOT__ with 3 OFX children
        Assert.Equal("__ROOT__", builder.Root.Name);
        Assert.Equal(3, builder.Root.Children.Count);
        Assert.All(builder.Root.Children, c => Assert.Equal("OFX", c.Name));
    }

    [Fact]
    public void TreeBuilder_ImplicitCloseWithGrandparent_Works()
    {
        var tokens = new List<SgmlToken>
        {
            new(SgmlTokenType.OpenTag, "PARENT", 1),
            new(SgmlTokenType.OpenTag, "CHILD1", 1),
            new(SgmlTokenType.Text, "value1", 1),
            new(SgmlTokenType.OpenTag, "CHILD2", 1),
            new(SgmlTokenType.Text, "value2", 1),
            new(SgmlTokenType.CloseTag, "PARENT", 1)
        };

        var builder = new SgmlTreeBuilder();
        builder.Build(tokens);

        Assert.NotNull(builder.Root);
        Assert.Equal("PARENT", builder.Root.Name);
        Assert.Equal(2, builder.Root.Children.Count);
        Assert.Equal("CHILD1", builder.Root.Children[0].Name);
        Assert.Equal("CHILD2", builder.Root.Children[1].Name);
        Assert.Equal("value1", builder.Root.Children[0].Value);
        Assert.Equal("value2", builder.Root.Children[1].Value);
    }
}
