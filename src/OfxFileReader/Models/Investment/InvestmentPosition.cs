namespace OfxFileReader.Models.Investment;

public sealed record InvestmentPosition(
    string SecurityId,
    string SecurityIdType,
    string HeldInAccount,
    string PositionType,
    decimal Units,
    decimal UnitPrice,
    decimal MarketValue,
    DateTimeOffset PriceAsOfDate,
    string? CurrencyCode = null,
    decimal? CurrencyRate = null,
    string? Memo = null
);
