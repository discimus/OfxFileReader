namespace OfxFileReader.Parsing.Sgml;

/// <summary>Builds a hierarchical SGML parse tree from a sequence of tokens.</summary>
internal sealed class SgmlTreeBuilder
{
    /// <summary>Gets the root node of the built parse tree, or null if empty.</summary>
    public SgmlNode? Root { get; private set; }

    /// <summary>Builds the parse tree from the given list of tokens.</summary>
    public void Build(List<SgmlToken> tokens)
    {
        Root = null;
        var stack = new Stack<SgmlNode>();

        foreach (var token in tokens)
        {
            switch (token.Type)
            {
                case SgmlTokenType.OpenTag:
                    var node = new SgmlNode(token.Value);

                    if (stack.Count > 0)
                    {
                        var parent = stack.Peek();

                        // If current top has a value, it's a leaf element that needs implicit closing
                        if (parent.Value is not null)
                        {
                            stack.Pop();
                            parent = stack.Count > 0 ? stack.Peek() : null;
                        }

                        if (parent is not null)
                        {
                            node.Parent = parent;
                            parent.Children.Add(node);
                        }
                    }
                    else
                    {
                        if (Root is null)
                        {
                            Root = node;
                        }
                        else
                        {
                            // Multiple root-level nodes — wrap in a virtual container
                            if (Root.Name == "__ROOT__")
                            {
                                // Already wrapped — just add as sibling
                                node.Parent = Root;
                                Root.Children.Add(node);
                            }
                            else
                            {
                                var wrapper = new SgmlNode("__ROOT__");
                                Root.Parent = wrapper;
                                wrapper.Children.Add(Root);
                                node.Parent = wrapper;
                                wrapper.Children.Add(node);
                                Root = wrapper;
                            }
                        }
                    }

                    stack.Push(node);
                    break;

                case SgmlTokenType.CloseTag:
                    // Pop until we find matching tag
                    var closed = false;
                    while (stack.Count > 0 && !closed)
                    {
                        var popped = stack.Pop();
                        if (string.Equals(popped.Name, token.Value, StringComparison.OrdinalIgnoreCase))
                        {
                            closed = true;
                        }
                    }
                    break;

                case SgmlTokenType.Text:
                    if (stack.Count > 0 && stack.Peek().Value is null)
                    {
                        stack.Peek().Value = token.Value;
                    }
                    break;
            }
        }
    }
}
