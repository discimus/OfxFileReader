namespace OfxFileReader.Parsing.Sgml;

/// <summary>Represents a node in the SGML parse tree, with a name, optional value, children, and parent reference.</summary>
internal sealed class SgmlNode
{
    /// <summary>Gets the tag name of this node.</summary>
    public string Name { get; }

    /// <summary>Gets or sets the text value of this node (for leaf elements).</summary>
    public string? Value { get; set; }

    /// <summary>Gets the list of child nodes.</summary>
    public List<SgmlNode> Children { get; } = [];

    /// <summary>Gets or sets the parent node.</summary>
    public SgmlNode? Parent { get; set; }

    /// <summary>Initializes a new instance with the specified tag name.</summary>
    public SgmlNode(string name)
    {
        Name = name;
    }

    /// <summary>Finds the first child node with the specified name (case-insensitive).</summary>
    public SgmlNode? FindChild(string name)
    {
        foreach (var child in Children)
        {
            if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                return child;
        }
        return null;
    }

    /// <summary>Finds all child nodes with the specified name (case-insensitive).</summary>
    public List<SgmlNode> FindChildren(string name)
    {
        var result = new List<SgmlNode>();
        foreach (var child in Children)
        {
            if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                result.Add(child);
        }
        return result;
    }

    /// <summary>Gets the value of the first child with the specified name.</summary>
    public string? GetChildValue(string name)
    {
        return FindChild(name)?.Value;
    }
}
