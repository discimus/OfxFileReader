using OfxFileReader.Logging;
using OfxFileReader.Models.Banking;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Parsing.Mappers;

internal static class BankMapper
{
    public static BankStatement? MapStatement(SgmlNode stmtrs, IOfxLogger logger)
    {
        try
        {
            var currency = stmtrs.GetChildValue("CURDEF");

            var account = MapAccount(stmtrs.FindChild("BANKACCTFROM"));
            if (account is null)
            {
                logger.LogWarning("BANKACCTFROM not found in STMTRS");
                return null;
            }

            var ledgerBal = ParseBalance(stmtrs.FindChild("LEDGERBAL"));
            var availBal = ParseBalance(stmtrs.FindChild("AVAILBAL"));
            var marketingInfo = stmtrs.GetChildValue("MKTGINFO");
            var trnUid = stmtrs.Parent?.GetChildValue("TRNUID");

            var tranList = stmtrs.FindChild("BANKTRANLIST");
            DateTimeOffset startDate = default, endDate = default;
            var transactions = new List<BankTransaction>();

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

            return new BankStatement(
                currency, account, ledgerBal, availBal,
                startDate, endDate, transactions.AsReadOnly(),
                marketingInfo, trnUid
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map bank statement", ex);
            return null;
        }
    }

    private static BankAccount? MapAccount(SgmlNode? acctNode)
    {
        if (acctNode is null) return null;

        var bankId = acctNode.GetChildValue("BANKID") ?? string.Empty;
        var acctId = acctNode.GetChildValue("ACCTID") ?? string.Empty;
        var acctTypeStr = acctNode.GetChildValue("ACCTTYPE");
        var branchId = acctNode.GetChildValue("BRANCHID");
        var acctKey = acctNode.GetChildValue("ACCTKEY");

        var acctType = acctTypeStr?.ToUpperInvariant() switch
        {
            "CHECKING" => AccountType.Checking,
            "SAVINGS" => AccountType.Savings,
            "MONEYMRKT" => AccountType.MoneyMarket,
            "CREDITLINE" => AccountType.LineOfCredit,
            _ => AccountType.Unknown
        };

        return new BankAccount(bankId, acctId, acctType, branchId, acctKey);
    }

    private static BankTransaction? MapTransaction(SgmlNode trn, IOfxLogger logger)
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
                logger.LogWarning("STMTTRN missing required fields: amount is null or FITID is null");
                return null;
            }

            return new BankTransaction(
                type,
                datePosted ?? DateTimeOffset.MinValue,
                dateUser, dateAvail,
                amount.Value, fitId,
                trn.GetChildValue("NAME"),
                trn.GetChildValue("MEMO"),
                trn.GetChildValue("CHECKNUM"),
                trn.GetChildValue("REFNUM"),
                trn.GetChildValue("PAYEEID"),
                trn.GetChildValue("SIC"),
                trn.GetChildValue("CURSYM"),
                OfxAmountConverter.Parse(trn.GetChildValue("CURRATE")),
                trn.GetChildValue("CORRECTFITID"),
                trn.GetChildValue("CORRECTIVEACTION"),
                trn.GetChildValue("SRVRTID")
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map bank transaction", ex);
            return null;
        }
    }

    public static Balance? ParseBalance(SgmlNode? balNode)
    {
        if (balNode is null) return null;

        var amount = OfxAmountConverter.Parse(balNode.GetChildValue("BALAMT"));
        var asOf = OfxDateConverter.Parse(balNode.GetChildValue("DTASOF"));

        if (amount is null)
            return null;

        return new Balance(amount.Value, asOf ?? DateTimeOffset.MinValue);
    }
}
