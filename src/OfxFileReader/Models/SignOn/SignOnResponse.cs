using OfxFileReader.Models.Common;

namespace OfxFileReader.Models.SignOn;

public sealed record SignOnResponse(
    Status Status,
    DateTimeOffset ServerDate,
    string? Language,
    DateTimeOffset? ProfileUpdateDate = null,
    DateTimeOffset? AccountUpdateDate = null,
    FinancialInstitution? FinancialInstitution = null,
    string? SessionCookie = null
);
