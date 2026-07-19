namespace OfxFileReader.Models.Common.Enums;

/// <summary>Defines the OFX specification version detected from the header.</summary>
public enum OfxVersion
{
    /// <summary>Unknown or undetermined OFX version.</summary>
    Unknown = 0,
    /// <summary>OFX version 1.x (SGML-based).</summary>
    V1x = 1,
    /// <summary>OFX version 2.x (XML-based).</summary>
    V2x = 2
}
