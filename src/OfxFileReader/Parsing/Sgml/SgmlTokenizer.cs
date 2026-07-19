using System.Text;

namespace OfxFileReader.Parsing.Sgml;

/// <summary>Tokenizes raw SGML OFX content into a sequence of tokens (open tags, close tags, text).</summary>
internal sealed class SgmlTokenizer
{
    private readonly string _content;
    private int _position;
    private int _lineNumber = 1;

    /// <summary>Initializes a new instance with the SGML content string.</summary>
    public SgmlTokenizer(string content)
    {
        _content = content;
    }

    /// <summary>Tokenizes the content and returns a list of tokens.</summary>
    public List<SgmlToken> Tokenize()
    {
        var tokens = new List<SgmlToken>();
        _position = 0;
        _lineNumber = 1;

        while (_position < _content.Length)
        {
            SkipWhitespace();

            if (_position >= _content.Length)
                break;

            var currentChar = _content[_position];

            if (currentChar == '<')
            {
                var tagStart = _position;
                var tagEnd = FindTagEnd();
                if (tagEnd < 0)
                    break;

                var tagContent = _content[(tagStart + 1)..tagEnd].Trim();
                var currentLine = _lineNumber;

                if (tagContent.StartsWith('/'))
                {
                    tokens.Add(new SgmlToken(SgmlTokenType.CloseTag, tagContent[1..].Trim(), currentLine));
                }
                else if (tagContent.EndsWith('/'))
                {
                    tokens.Add(new SgmlToken(SgmlTokenType.OpenTag, tagContent[..^1].Trim(), currentLine));
                }
                else
                {
                    tokens.Add(new SgmlToken(SgmlTokenType.OpenTag, tagContent, currentLine));
                }
            }
            else
            {
                var textEnd = FindTextEnd();
                if (textEnd > _position)
                {
                    var text = _content[_position..textEnd].Trim();
                    var currentLine = _lineNumber;

                    if (text.Length > 0)
                    {
                        tokens.Add(new SgmlToken(SgmlTokenType.Text, text, currentLine));
                    }
                }
                _position = textEnd;
            }
        }

        return tokens;
    }

    /// <summary>Finds the end of a tag (the closing &gt; character).</summary>
    private int FindTagEnd()
    {
        var pos = _position;
        while (pos < _content.Length)
        {
            if (_content[pos] == '>')
            {
                _position = pos + 1;
                return pos;
            }

            if (_content[pos] == '\n')
                _lineNumber++;

            pos++;
        }
        return -1;
    }

    /// <summary>Finds the end of a text segment (before the next &lt; character).</summary>
    private int FindTextEnd()
    {
        for (var pos = _position; pos < _content.Length; pos++)
        {
            if (_content[pos] == '<')
                return pos;

            if (_content[pos] == '\n')
                _lineNumber++;
        }
        return _content.Length;
    }

    /// <summary>Advances the position past any whitespace characters.</summary>
    private void SkipWhitespace()
    {
        while (_position < _content.Length && char.IsWhiteSpace(_content[_position]))
        {
            if (_content[_position] == '\n')
                _lineNumber++;
            _position++;
        }
    }

    /// <summary>Placeholder for counting newlines in text (currently unused).</summary>
    private static void CountNewlines(string text)
    {
        // Line counting is done inline; this is a placeholder for clarity
    }
}
