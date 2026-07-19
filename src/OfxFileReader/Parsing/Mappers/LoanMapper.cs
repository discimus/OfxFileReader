using OfxFileReader.Logging;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Models.Loan;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Parsing.Mappers;

/// <summary>Maps SGML parse tree nodes for loan data into domain models.</summary>
internal static class LoanMapper
{
    /// <summary>Maps a LOANSTMTRS SGML node to a <see cref="LoanStatement"/>.</summary>
    public static LoanStatement? MapStatement(SgmlNode loanmtrs, IOfxLogger logger)
    {
        try
        {
            var currency = loanmtrs.GetChildValue("CURDEF");

            var account = MapAccount(loanmtrs.FindChild("LOANACCTFROM"));
            if (account is null)
            {
                logger.LogWarning("LOANACCTFROM not found in LOANSTMTRS");
                return null;
            }

            var principalBal = BankMapper.ParseBalance(loanmtrs.FindChild("PRINCIPALBAL"));
            var ledgerBal = BankMapper.ParseBalance(loanmtrs.FindChild("LEDGERBAL"));
            var availBal = BankMapper.ParseBalance(loanmtrs.FindChild("AVAILBAL"));

            var loanInfo = loanmtrs.FindChild("LOANINFO");
            decimal? interestRate = null;
            decimal? interestYtd = null;
            DateTimeOffset? nextPaymentDate = null;
            decimal? nextPaymentAmount = null;

            if (loanInfo is not null)
            {
                interestRate = OfxAmountConverter.Parse(loanInfo.GetChildValue("LOANINTRATE"));
                interestYtd = OfxAmountConverter.Parse(loanInfo.GetChildValue("LOANINTYTD"));
                nextPaymentDate = OfxDateConverter.Parse(loanInfo.GetChildValue("DTPAYMENTDUE"));
                nextPaymentAmount = OfxAmountConverter.Parse(loanInfo.GetChildValue("PAYMENTAMT"));
            }

            var trnUid = loanmtrs.Parent?.GetChildValue("TRNUID");

            var tranList = loanmtrs.FindChild("LOANTRANLIST");
            DateTimeOffset startDate = default, endDate = default;
            var transactions = new List<LoanTransaction>();

            if (tranList is not null)
            {
                startDate = OfxDateConverter.Parse(tranList.GetChildValue("DTSTART")) ?? default;
                endDate = OfxDateConverter.Parse(tranList.GetChildValue("DTEND")) ?? default;

                foreach (var trn in tranList.FindChildren("LOANTRN"))
                {
                    var tx = MapTransaction(trn, logger);
                    if (tx is not null)
                        transactions.Add(tx);
                }
            }

            return new LoanStatement(
                currency, account,
                principalBal, ledgerBal, availBal,
                startDate, endDate,
                interestRate, interestYtd,
                nextPaymentDate, nextPaymentAmount,
                null, // PayToAddress - complex, skip for now
                transactions.AsReadOnly(),
                trnUid
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map loan statement", ex);
            return null;
        }
    }

    /// <summary>Maps a LOANACCTFROM SGML node to a <see cref="LoanAccount"/>.</summary>
    private static LoanAccount? MapAccount(SgmlNode? acctNode)
    {
        if (acctNode is null) return null;
        var bankId = acctNode.GetChildValue("BANKID") ?? string.Empty;
        var acctId = acctNode.GetChildValue("ACCTID") ?? string.Empty;
        var loanType = acctNode.GetChildValue("LOANTYPE")?.ToUpperInvariant() switch
        {
            "FIXED" => AccountType.Loan,
            "LINE" => AccountType.LineOfCredit,
            "VLN" => AccountType.Loan,
            _ => AccountType.Loan
        };
        return new LoanAccount(bankId, acctId, loanType);
    }

    /// <summary>Maps a LOANTRN SGML node to a <see cref="LoanTransaction"/>.</summary>
    private static LoanTransaction? MapTransaction(SgmlNode trn, IOfxLogger logger)
    {
        try
        {
            var type = TransactionTypeConverter.Parse(trn.GetChildValue("TRNTYPE"));
            var datePosted = OfxDateConverter.Parse(trn.GetChildValue("DTPOSTED"));
            var amount = OfxAmountConverter.Parse(trn.GetChildValue("TRNAMT"));
            var fitId = trn.GetChildValue("FITID");

            if (amount is null || fitId is null || datePosted is null)
            {
                logger.LogWarning($"LOANTRN missing required fields");
                return null;
            }

            return new LoanTransaction(
                type, datePosted.Value, amount.Value, fitId,
                trn.GetChildValue("NAME"),
                trn.GetChildValue("MEMO"),
                OfxAmountConverter.Parse(trn.GetChildValue("PRINCIPALAMT")),
                OfxAmountConverter.Parse(trn.GetChildValue("INTERESTAMT")),
                OfxAmountConverter.Parse(trn.GetChildValue("ESCROWAMT")),
                trn.GetChildValue("CURSYM"),
                OfxAmountConverter.Parse(trn.GetChildValue("CURRATE"))
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map loan transaction", ex);
            return null;
        }
    }
}
