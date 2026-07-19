using OfxFileReader.Logging;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.CreditCard;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Parsing.Mappers;

internal static class CreditCardMapper
{
    public static CreditCardStatement? MapStatement(SgmlNode ccmtrs, IOfxLogger logger)
    {
        try
        {
            var currency = ccmtrs.GetChildValue("CURDEF");

            var account = MapAccount(ccmtrs.FindChild("CCACCTFROM"));
            if (account is null)
            {
                logger.LogWarning("CCACCTFROM not found in CCSTMTRS");
                return null;
            }

            var ledgerBal = BankMapper.ParseBalance(ccmtrs.FindChild("LEDGERBAL"));
            var availBal = BankMapper.ParseBalance(ccmtrs.FindChild("AVAILBAL"));
            var marketingInfo = ccmtrs.GetChildValue("MKTGINFO");
            var trnUid = ccmtrs.Parent?.GetChildValue("TRNUID");

            var tranList = ccmtrs.FindChild("BANKTRANLIST");
            DateTimeOffset startDate = default, endDate = default;
            var transactions = new List<CreditCardTransaction>();

            if (tranList is not null)
            {
                startDate = OfxDateConverter.Parse(tranList.GetChildValue("DTSTART")) ?? default;
                endDate = OfxDateConverter.Parse(tranList.GetChildValue("DTEND")) ?? default;

                foreach (var trn in tranList.FindChildren("STMTTRN"))
                {
                    var tx = MapTransaction(trn, logger);
                    if (tx is not null)
                        transactions.Add(tx);
                }
            }

            return new CreditCardStatement(
                currency, account, ledgerBal, availBal,
                startDate, endDate, transactions.AsReadOnly(),
                marketingInfo, trnUid
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map credit card statement", ex);
            return null;
        }
    }

    private static CreditCardAccount? MapAccount(SgmlNode? acctNode)
    {
        if (acctNode is null) return null;
        var acctId = acctNode.GetChildValue("ACCTID") ?? string.Empty;
        return new CreditCardAccount(acctId);
    }

    private static CreditCardTransaction? MapTransaction(SgmlNode trn, IOfxLogger logger)
    {
        try
        {
            var type = TransactionTypeConverter.Parse(trn.GetChildValue("TRNTYPE"));
            var datePosted = OfxDateConverter.Parse(trn.GetChildValue("DTPOSTED"));
            var dateUser = OfxDateConverter.Parse(trn.GetChildValue("DTUSER"));
            var dateAvail = OfxDateConverter.Parse(trn.GetChildValue("DTAVAIL"));
            var amount = OfxAmountConverter.Parse(trn.GetChildValue("TRNAMT"));
            var fitId = trn.GetChildValue("FITID");

            if (amount is null || fitId is null)
            {
                logger.LogWarning("CC STMTTRN missing required fields: amount is null or FITID is null");
                return null;
            }

            return new CreditCardTransaction(
                type,
                datePosted ?? DateTimeOffset.MinValue,
                dateUser, dateAvail,
                amount.Value, fitId,
                trn.GetChildValue("NAME"),
                trn.GetChildValue("MEMO"),
                trn.GetChildValue("REFNUM"),
                trn.GetChildValue("PAYEEID"),
                trn.GetChildValue("SIC"),
                trn.GetChildValue("CURSYM"),
                OfxAmountConverter.Parse(trn.GetChildValue("CURRATE"))
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map credit card transaction", ex);
            return null;
        }
    }
}
