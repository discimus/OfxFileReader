namespace OfxFileReader.Parsing.Sgml;

internal sealed class SgmlNode
{
    public string Name { get; }
    public string? Value { get; set; }
    public List<SgmlNode> Children { get; } = [];
    public SgmlNode? Parent { get; set; }

    public SgmlNode(string name)
    {
        Name = name;
    }

    public SgmlNode? FindChild(string name)
    {
        foreach (var child in Children)
        {
            if (string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                return child;
        }
        return null;
    }

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

    public string? GetChildValue(string name)
    {
        return FindChild(name)?.Value;
    }
}
