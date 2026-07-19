using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models;

public sealed record OfxHeader(
    int OfxHeaderValue,
    string Data,
    OfxVersion Version,
    string Security,
    string Encoding,
    int Charset,
    string Compression,
    string OldFileUid,
    string NewFileUid
);
