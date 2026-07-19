using OfxFileReader.Models;

namespace OfxFileReader.Parsing;

/// <summary>Defines the contract for an OFX content parser strategy.</summary>
internal interface IOfxParser
{
    /// <summary>Parses the raw OFX content string into a structured document.</summary>
    OfxDocument Parse(string content);
}
