using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Common;

/// <summary>Represents a status code, severity, and optional message from an OFX response.</summary>
/// <param name="Code">The numeric status code.</param>
/// <param name="Severity">The severity level (INFO, WARN, ERROR).</param>
/// <param name="Message">Optional human-readable status message.</param>
public sealed record Status(int Code, SeverityType Severity, string? Message = null);
