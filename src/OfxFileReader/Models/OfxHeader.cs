using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models;

/// <summary>Represents the header metadata of an OFX file.</summary>
public sealed record OfxHeader(
    /// <summary>The OFX header version number (e.g., 100, 200).</summary>
    int OfxHeaderValue,
    /// <summary>The data format type (e.g., OFXSGML, OFXXML).</summary>
    string Data,
    /// <summary>The detected OFX specification version.</summary>
    OfxVersion Version,
    /// <summary>The security protocol used (e.g., NONE, TYPE1).</summary>
    string Security,
    /// <summary>The character encoding of the file (e.g., USASCII, UTF-8).</summary>
    string Encoding,
    /// <summary>The charset code (e.g., 1252 for Windows, 65001 for UTF-8).</summary>
    int Charset,
    /// <summary>The compression type (e.g., NONE, PKZIP).</summary>
    string Compression,
    /// <summary>The UID of the previous file for file replacement tracking.</summary>
    string OldFileUid,
    /// <summary>The UID of the current file for file replacement tracking.</summary>
    string NewFileUid
);
