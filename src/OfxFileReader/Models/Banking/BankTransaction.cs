using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Models.Banking;

public sealed record BankTransaction(
    TransactionType Type,
    DateTimeOffset DatePosted,
    DateTimeOffset? DateUser,
    DateTimeOffset? DateAvailable,
    decimal Amount,
    string FitId,
    string? Name = null,
    string? Memo = null,
    string? CheckNumber = null,
    string? ReferenceNumber = null,
    string? PayeeId = null,
    string? Sic = null,
    string? CurrencyCode = null,
    decimal? CurrencyRate = null,
    string? CorrectFitId = null,
    string? CorrectiveAction = null,
    string? ServerTransactionId = null
);
