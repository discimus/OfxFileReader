namespace OfxFileReader.Models.Common.Enums;

/// <summary>Defines the severity levels for OFX status codes.</summary>
public enum SeverityType
{
    /// <summary>Unknown or unidentifiable severity level.</summary>
    Unknown = 0,
    /// <summary>Informational message.</summary>
    Info,
    /// <summary>Warning — the request may have partially succeeded.</summary>
    Warn,
    /// <summary>Error — the request failed.</summary>
    Error
}
