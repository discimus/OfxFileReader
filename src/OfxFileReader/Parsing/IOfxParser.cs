using OfxFileReader.Models;

namespace OfxFileReader.Parsing;

internal interface IOfxParser
{
    OfxDocument Parse(string content);
}
