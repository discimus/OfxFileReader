using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Common;

public sealed record Status(int Code, SeverityType Severity, string? Message = null);
