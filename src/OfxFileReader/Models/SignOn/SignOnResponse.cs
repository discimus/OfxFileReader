using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.SignOn;

/// <summary>Represents the sign-on response (SONRS) from the OFX server.</summary>
public sealed record SignOnResponse(
    /// <summary>The status of the sign-on request.</summary>
    Status Status,
    /// <summary>The server date and time from the OFX response.</summary>
    DateTimeOffset ServerDate,
    /// <summary>The language of the OFX response (e.g., ENG).</summary>
    string? Language,
    /// <summary>The date of the last profile update, if available.</summary>
    DateTimeOffset? ProfileUpdateDate = null,
    /// <summary>The date of the last account update, if available.</summary>
    DateTimeOffset? AccountUpdateDate = null,
    /// <summary>The financial institution information, if provided.</summary>
    FinancialInstitution? FinancialInstitution = null,
    /// <summary>The session cookie for subsequent requests, if provided.</summary>
    string? SessionCookie = null
);
