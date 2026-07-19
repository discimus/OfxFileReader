using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Investment;

public sealed record InvestmentTransaction(
    string FitId,
    DateTimeOffset DatePosted,
    DateTimeOffset? DateSettle,
    InvestmentTransactionType Type,
    decimal? Units = null,
    decimal? UnitPrice = null,
    decimal? TotalAmount = null,
    decimal? Commission = null,
    decimal? Fees = null,
    decimal? Load = null,
    decimal? Interest = null,
    decimal? Gain = null,
    string? SecurityId = null,
    string? SecurityIdType = null,
    string? Memo = null,
    string? CurrencyCode = null,
    decimal? CurrencyRate = null,
    string? Inv401kSource = null
);
