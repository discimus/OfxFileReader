using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.CreditCard;

public sealed record CreditCardTransaction(
    TransactionType Type,
    DateTimeOffset DatePosted,
    DateTimeOffset? DateUser,
    DateTimeOffset? DateAvailable,
    decimal Amount,
    string FitId,
    string? Name = null,
    string? Memo = null,
    string? ReferenceNumber = null,
    string? PayeeId = null,
    string? Sic = null,
    string? CurrencyCode = null,
    decimal? CurrencyRate = null
);
