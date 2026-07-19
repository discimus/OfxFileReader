using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Tests.Parsing;

public class SgmlTreeBuilderTests
{
    [Fact]
    public void Build_SimpleTags_CreatesCorrectTree()
    {
        var tokens = new List<SgmlToken>
        {
            new(SgmlTokenType.OpenTag, "ROOT", 1),
            new(SgmlTokenType.Text, "value", 1),
            new(SgmlTokenType.CloseTag, "ROOT", 1)
        };

        var builder = new SgmlTreeBuilder();
        builder.Build(tokens);

        Assert.NotNull(builder.Root);
        Assert.Equal("ROOT", builder.Root.Name);
        Assert.Equal("value", builder.Root.Value);
    }

    [Fact]
    public void Build_ImplicitClose_HandlesCorrectly()
    {
        // SGML pattern: sibling leaves without closing tags
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
        Assert.Equal("value1", builder.Root.Children[0].Value);
        Assert.Equal("CHILD2", builder.Root.Children[1].Name);
        Assert.Equal("value2", builder.Root.Children[1].Value);
    }

    [Fact]
    public void Build_NestedContainers_CreatesCorrectHierarchy()
    {
        var tokens = new List<SgmlToken>
        {
            new(SgmlTokenType.OpenTag, "OUTER", 1),
            new(SgmlTokenType.OpenTag, "INNER", 1),
            new(SgmlTokenType.OpenTag, "LEAF", 1),
            new(SgmlTokenType.Text, "value", 1),
            new(SgmlTokenType.CloseTag, "INNER", 1),
            new(SgmlTokenType.CloseTag, "OUTER", 1)
        };

        var builder = new SgmlTreeBuilder();
        builder.Build(tokens);

        Assert.NotNull(builder.Root);
        Assert.Equal("OUTER", builder.Root.Name);
        Assert.Single(builder.Root.Children);
        Assert.Equal("INNER", builder.Root.Children[0].Name);
        Assert.Single(builder.Root.Children[0].Children);
        Assert.Equal("LEAF", builder.Root.Children[0].Children[0].Name);
        Assert.Equal("value", builder.Root.Children[0].Children[0].Value);
    }

    [Fact]
    public void Build_EmptyTokens_NoRoot()
    {
        var builder = new SgmlTreeBuilder();
        builder.Build([]);
        Assert.Null(builder.Root);
    }
}
