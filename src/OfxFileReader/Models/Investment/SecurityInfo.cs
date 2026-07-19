namespace OfxFileReader.Models.Investment;

public sealed record SecurityInfo(
    string SecurityId,
    string SecurityIdType,
    string? SecurityName = null,
    string? Ticker = null,
    string? FiName = null,
    string? UnitType = null,
    string? SecurityType = null,
    decimal? Rating = null
);
