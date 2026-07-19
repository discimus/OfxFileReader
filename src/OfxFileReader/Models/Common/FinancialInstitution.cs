namespace OfxFileReader.Models.Common;

/// <summary>Represents a financial institution (FI) from an OFX sign-on response.</summary>
/// <param name="Organization">The name of the financial institution.</param>
/// <param name="Fid">The financial institution identifier (FID).</param>
public sealed record FinancialInstitution(string? Organization = null, string? Fid = null);
