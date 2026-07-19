using OfxFileReader.Logging;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.Investment;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Parsing.Mappers;

internal static class InvestmentMapper
{
    public static InvestmentStatement? MapStatement(SgmlNode invmtrs, IOfxLogger logger)
    {
        try
        {
            var asOfDate = OfxDateConverter.Parse(invmtrs.GetChildValue("DTASOF")) ?? DateTimeOffset.MinValue;
            var currency = invmtrs.GetChildValue("CURDEF");

            var account = MapAccount(invmtrs.FindChild("INVACCTFROM"));
            if (account is null)
            {
                logger.LogWarning("INVACCTFROM not found in INVSTMTRS");
                return null;
            }

            var invBal = invmtrs.FindChild("INVBAL");
            Balance? availCash = null;
            decimal? marginBalance = null;
            decimal? shortBalance = null;

            if (invBal is not null)
            {
                var availCashAmt = OfxAmountConverter.Parse(invBal.GetChildValue("AVAILCASH"));
                var marginAmt = OfxAmountConverter.Parse(invBal.GetChildValue("MARGINBALANCE"));
                var shortAmt = OfxAmountConverter.Parse(invBal.GetChildValue("SHORTBALANCE"));

                if (availCashAmt.HasValue)
                    availCash = new Balance(availCashAmt.Value, asOfDate);

                marginBalance = marginAmt;
                shortBalance = shortAmt;
            }

            var trnUid = invmtrs.Parent?.GetChildValue("TRNUID");

            var tranList = invmtrs.FindChild("INVTRANLIST");
            DateTimeOffset? startDate = null, endDate = null;
            var transactions = new List<InvestmentTransaction>();

            if (tranList is not null)
            {
                startDate = OfxDateConverter.Parse(tranList.GetChildValue("DTSTART"));
                endDate = OfxDateConverter.Parse(tranList.GetChildValue("DTEND"));

                foreach (var trn in tranList.FindChildren("INVTRAN"))
                    AddBuySell(trn, transactions, logger);

                foreach (var trn in tranList.FindChildren("BUYSTOCK"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("SELLSTOCK"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("BUYMF"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("SELLMF"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("BUYOPT"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("SELLOPT"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("BUYOTHER"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("SELLOTHER"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("REINVEST"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("RETOFCAP"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("SPLIT"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("INCOME"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("EXPENSE"))
                    AddBuySell(trn, transactions, logger);
                foreach (var trn in tranList.FindChildren("TRANSFER"))
                    AddBuySell(trn, transactions, logger);
            }

            var positions = new List<InvestmentPosition>();
            var posList = invmtrs.FindChild("INVPOSLIST");
            if (posList is not null)
            {
                foreach (var pos in posList.FindChildren("INVPOS"))
                {
                    var position = MapPosition(pos, logger);
                    if (position is not null)
                        positions.Add(position);
                }
            }

            var securities = new List<SecurityInfo>();
            var secList = invmtrs.FindChild("SECLIST");
            if (secList is not null)
            {
                foreach (var secInfo in secList.Children)
                {
                    var secIdNode = secInfo.FindChild("SECID");
                    var secId = secIdNode?.GetChildValue("UNIQUEID") ?? string.Empty;
                    var secIdType = secIdNode?.GetChildValue("UNIQUEIDTYPE") ?? string.Empty;

                    var security = new SecurityInfo(
                        secId, secIdType,
                        secInfo.GetChildValue("SECNAME"),
                        secInfo.GetChildValue("TICKER"),
                        secInfo.GetChildValue("FINAME"),
                        secInfo.GetChildValue("UNITTYPE"),
                        secInfo.Name,
                        OfxAmountConverter.Parse(secInfo.GetChildValue("RATING"))
                    );
                    securities.Add(security);
                }
            }

            return new InvestmentStatement(
                asOfDate, currency, account,
                availCash, marginBalance, shortBalance,
                startDate, endDate,
                positions.AsReadOnly(),
                transactions.AsReadOnly(),
                securities.AsReadOnly(),
                trnUid
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map investment statement", ex);
            return null;
        }
    }

    private static InvestmentAccount? MapAccount(SgmlNode? acctNode)
    {
        if (acctNode is null) return null;
        var brokerId = acctNode.GetChildValue("BROKERID") ?? string.Empty;
        var acctId = acctNode.GetChildValue("ACCTID") ?? string.Empty;
        return new InvestmentAccount(brokerId, acctId);
    }

    private static void AddBuySell(SgmlNode trn, List<InvestmentTransaction> transactions, IOfxLogger logger)
    {
        var tx = MapInvestmentTransaction(trn, logger);
        if (tx is not null)
            transactions.Add(tx);
    }

    private static InvestmentTransaction? MapInvestmentTransaction(SgmlNode trn, IOfxLogger logger)
    {
        try
        {
            var invTran = trn.FindChild("INVTRAN");
            var fitId = invTran?.GetChildValue("FITID") ?? trn.GetChildValue("FITID");
            var datePosted = OfxDateConverter.Parse(invTran?.GetChildValue("DTPOSTED") ?? trn.GetChildValue("DTPOSTED"));
            var dateSettle = OfxDateConverter.Parse(invTran?.GetChildValue("DTSETTLE") ?? trn.GetChildValue("DTSETTLE"));

            if (fitId is null)
            {
                logger.LogWarning("Investment transaction missing FITID");
                return null;
            }

            var invTypeStr = trn.Name;
            var invType = InvestmentTransactionTypeConverter.Parse(invTypeStr);
            // Fallback: if the type wasn't recognized from the tag name, try the explicit tag
            if (invType == Models.Common.Enums.InvestmentTransactionType.Unknown)
                invType = InvestmentTransactionTypeConverter.Parse(trn.GetChildValue("INVTRANFOR"));

            var secIdNode = trn.FindChild("SECID");
            var secId = secIdNode?.GetChildValue("UNIQUEID");
            var secIdType = secIdNode?.GetChildValue("UNIQUEIDTYPE");

            return new InvestmentTransaction(
                fitId,
                datePosted ?? DateTimeOffset.MinValue,
                dateSettle,
                invType,
                OfxAmountConverter.Parse(trn.GetChildValue("UNITS")),
                OfxAmountConverter.Parse(trn.GetChildValue("UNITPRICE")),
                OfxAmountConverter.Parse(trn.GetChildValue("TOTAL")),
                OfxAmountConverter.Parse(trn.GetChildValue("COMMISSION")),
                OfxAmountConverter.Parse(trn.GetChildValue("FEES")),
                OfxAmountConverter.Parse(trn.GetChildValue("LOAD")),
                OfxAmountConverter.Parse(trn.GetChildValue("INTEREST")),
                OfxAmountConverter.Parse(trn.GetChildValue("GAIN")),
                secId, secIdType,
                trn.GetChildValue("MEMO"),
                trn.GetChildValue("CURSYM"),
                OfxAmountConverter.Parse(trn.GetChildValue("CURRATE")),
                trn.GetChildValue("INV401KSOURCE")
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map investment transaction", ex);
            return null;
        }
    }

    private static InvestmentPosition? MapPosition(SgmlNode pos, IOfxLogger logger)
    {
        try
        {
            var secIdNode = pos.FindChild("SECID");
            var secId = secIdNode?.GetChildValue("UNIQUEID") ?? string.Empty;
            var secIdType = secIdNode?.GetChildValue("UNIQUEIDTYPE") ?? string.Empty;
            var heldIn = pos.GetChildValue("HELDINACCT") ?? string.Empty;
            var posType = pos.GetChildValue("POSTYPE") ?? string.Empty;
            var units = OfxAmountConverter.Parse(pos.GetChildValue("UNITS"));
            var unitPrice = OfxAmountConverter.Parse(pos.GetChildValue("UNITPRICE"));
            var mktVal = OfxAmountConverter.Parse(pos.GetChildValue("MKTVAL"));
            var dtPriceAsOf = OfxDateConverter.Parse(pos.GetChildValue("DTPRICEASOF"));

            if (units is null || mktVal is null)
            {
                logger.LogWarning($"INVPOS missing required numeric fields");
                return null;
            }

            return new InvestmentPosition(
                secId, secIdType, heldIn, posType,
                units.Value, unitPrice ?? 0, mktVal.Value,
                dtPriceAsOf ?? DateTimeOffset.MinValue,
                pos.GetChildValue("CURSYM"),
                OfxAmountConverter.Parse(pos.GetChildValue("CURRATE")),
                pos.GetChildValue("MEMO")
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map investment position", ex);
            return null;
        }
    }
}
