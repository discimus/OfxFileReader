using System.Xml.Linq;
using OfxFileReader.Exceptions;
using OfxFileReader.Logging;
using OfxFileReader.Models;
using OfxFileReader.Models.Banking;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Models.CreditCard;
using OfxFileReader.Models.Investment;
using OfxFileReader.Models.Loan;
using OfxFileReader.Models.SignOn;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Mappers;

namespace OfxFileReader.Parsing.Xml;

/// <summary>Parses XML-format OFX content into an <see cref="OfxDocument"/>.</summary>
internal sealed class XmlOfxParser
{
    private readonly IOfxLogger _logger;

    /// <summary>Initializes a new instance with an optional logger.</summary>
    public XmlOfxParser(IOfxLogger? logger = null)
    {
        _logger = logger ?? NullOfxLogger.Instance;
    }

    /// <summary>Parses XML OFX content into a structured document.</summary>
    public OfxDocument Parse(string content)
    {
        var headerResult = HeaderParser.Parse(content);
        var header = headerResult.Header;

        // Remove XML declaration if present (<?xml ...?>)
        var cleanContent = System.Text.RegularExpressions.Regex.Replace(
            content, @"<\?xml[^>]*\?>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        XDocument doc;
        try
        {
            using var stringReader = new StringReader(cleanContent);
            using var xmlReader = System.Xml.XmlReader.Create(stringReader, new System.Xml.XmlReaderSettings
            {
                DtdProcessing = System.Xml.DtdProcessing.Prohibit,
                XmlResolver = null
            });
            doc = XDocument.Load(xmlReader);
        }
        catch (Exception ex)
        {
            throw new OfxParseException("Failed to parse XML content", ex);
        }

        var ofxElement = doc.Root;
        if (ofxElement is null || !string.Equals(ofxElement.Name.LocalName, "OFX", StringComparison.OrdinalIgnoreCase))
            throw new OfxParseException("Root <OFX> element not found");

        var signOn = ParseSignOn(ofxElement);
        var bankStatements = ParseBankStatements(ofxElement);
        var creditCardStatements = ParseCreditCardStatements(ofxElement);
        var investmentStatements = ParseInvestmentStatements(ofxElement);
        var loanStatements = ParseLoanStatements(ofxElement);

        var totalTransactions = (bankStatements?.Sum(s => s.Transactions.Count) ?? 0)
            + (creditCardStatements?.Sum(s => s.Transactions.Count) ?? 0)
            + (investmentStatements?.Sum(s => s.Transactions?.Count ?? 0) ?? 0)
            + (loanStatements?.Sum(s => s.Transactions?.Count ?? 0) ?? 0);

        var totalStatements = (bankStatements?.Count ?? 0)
            + (creditCardStatements?.Count ?? 0)
            + (investmentStatements?.Count ?? 0)
            + (loanStatements?.Count ?? 0);

        var metadata = new OfxParseMetadata(
            ParsedAt: DateTimeOffset.UtcNow,
            ParserVersion: "1.0.0",
            DetectedEncoding: header.Encoding,
            TransactionCount: totalTransactions,
            StatementCount: totalStatements
        );

        return new OfxDocument(
            header, metadata,
            signOn,
            bankStatements?.AsReadOnly(),
            creditCardStatements?.AsReadOnly(),
            investmentStatements?.AsReadOnly(),
            loanStatements?.AsReadOnly()
        );
    }

    /// <summary>Gets the first child element with the specified local name (case-insensitive).</summary>
    private static XElement? GetChild(XElement parent, string name)
    {
        return parent.Elements().FirstOrDefault(e =>
            string.Equals(e.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Gets all child elements with the specified local name (case-insensitive).</summary>
    private static IEnumerable<XElement> GetChildren(XElement parent, string name)
    {
        return parent.Elements().Where(e =>
            string.Equals(e.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Gets the trimmed text value of an element, or null if the element is null.</summary>
    private static string? GetValue(XElement? element)
    {
        return element?.Value?.Trim();
    }

    /// <summary>Gets the trimmed text value of the first child element with the specified name.</summary>
    private static string? GetChildValue(XElement parent, string name)
    {
        return GetValue(GetChild(parent, name));
    }

    /// <summary>Parses the sign-on response section from the OFX XML element.</summary>
    private SignOnResponse? ParseSignOn(XElement ofx)
    {
        var signonMsgs = GetChild(ofx, "SIGNONMSGSRSV1");
        if (signonMsgs is null) return null;

        var sonrs = GetChild(signonMsgs, "SONRS");
        if (sonrs is null)
        {
            _logger.LogWarning("SONRS not found in SIGNONMSGSRSV1");
            return null;
        }

        return MapSignOn(sonrs);
    }

    /// <summary>Maps a SONRS XML element to a <see cref="SignOnResponse"/>.</summary>
    private SignOnResponse MapSignOn(XElement sonrs)
    {
        var statusNode = GetChild(sonrs, "STATUS");
        var code = int.TryParse(GetChildValue(statusNode, "CODE"), out var c) ? c : 0;
        var severity = SeverityTypeConverter.Parse(GetChildValue(statusNode, "SEVERITY"));
        var statusMsg = GetChildValue(statusNode, "MESSAGE");

        var serverDate = OfxDateConverter.Parse(GetChildValue(sonrs, "DTSERVER")) ?? DateTimeOffset.MinValue;
        var language = GetChildValue(sonrs, "LANGUAGE") ?? "ENG";

        var fiNode = GetChild(sonrs, "FI");
        FinancialInstitution? fi = fiNode is not null
            ? new FinancialInstitution(GetChildValue(fiNode, "ORG"), GetChildValue(fiNode, "FID"))
            : null;

        return new SignOnResponse(
            new Status(code, severity, statusMsg),
            serverDate, language,
            OfxDateConverter.Parse(GetChildValue(sonrs, "DTPROFUP")),
            OfxDateConverter.Parse(GetChildValue(sonrs, "DTACCTUP")),
            fi,
            GetChildValue(sonrs, "SESSCOOKIE")
        );
    }

    /// <summary>Parses bank statement sections from the OFX XML element.</summary>
    private List<BankStatement>? ParseBankStatements(XElement ofx)
    {
        var bankMsgs = GetChild(ofx, "BANKMSGSRSV1");
        if (bankMsgs is null) return null;

        var statements = new List<BankStatement>();
        foreach (var trn in GetChildren(bankMsgs, "STMTTRNRS"))
        {
            var stmtrs = GetChild(trn, "STMTRS");
            if (stmtrs is null) continue;

            var stmt = MapBankStatement(stmtrs, trn);
            if (stmt is not null)
                statements.Add(stmt);
        }
        return statements;
    }

    /// <summary>Maps an STMTRS XML element to a <see cref="BankStatement"/>.</summary>
    private BankStatement? MapBankStatement(XElement stmtrs, XElement parent)
    {
        try
        {
            var currency = GetChildValue(stmtrs, "CURDEF") ?? "USD";
            var acct = ParseBankAccount(GetChild(stmtrs, "BANKACCTFROM"));
            if (acct is null) return null;

            var trnList = GetChild(stmtrs, "BANKTRANLIST");
            var startDate = trnList is not null
                ? OfxDateConverter.Parse(GetChildValue(trnList, "DTSTART")) ?? DateTimeOffset.MinValue
                : DateTimeOffset.MinValue;
            var endDate = trnList is not null
                ? OfxDateConverter.Parse(GetChildValue(trnList, "DTEND")) ?? DateTimeOffset.MinValue
                : DateTimeOffset.MinValue;

            var transactions = new List<BankTransaction>();
            if (trnList is not null)
            {
                foreach (var trn in GetChildren(trnList, "STMTTRN"))
                {
                    var tx = MapBankTransaction(trn);
                    if (tx is not null)
                        transactions.Add(tx);
                }
            }

            return new BankStatement(
                currency, acct,
                ParseBalanceElement(GetChild(stmtrs, "LEDGERBAL")),
                ParseBalanceElement(GetChild(stmtrs, "AVAILBAL")),
                startDate, endDate,
                transactions.AsReadOnly(),
                GetChildValue(stmtrs, "MKTGINFO"),
                GetChildValue(parent, "TRNUID")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to map bank statement from XML", ex);
            return null;
        }
    }

    /// <summary>Parses a BANKACCTFROM XML element into a <see cref="BankAccount"/>.</summary>
    private BankAccount? ParseBankAccount(XElement? acct)
    {
        if (acct is null) return null;
        var acctType = GetChildValue(acct, "ACCTTYPE")?.ToUpperInvariant() switch
        {
            "CHECKING" => AccountType.Checking,
            "SAVINGS" => AccountType.Savings,
            "MONEYMRKT" => AccountType.MoneyMarket,
            "CREDITLINE" => AccountType.LineOfCredit,
            _ => AccountType.Unknown
        };
        return new BankAccount(
            GetChildValue(acct, "BANKID") ?? string.Empty,
            GetChildValue(acct, "ACCTID") ?? string.Empty,
            acctType,
            GetChildValue(acct, "BRANCHID"),
            GetChildValue(acct, "ACCTKEY")
        );
    }

    /// <summary>Maps an STMTTRN XML element to a <see cref="BankTransaction"/>.</summary>
    private BankTransaction? MapBankTransaction(XElement trn)
    {
        var amount = OfxAmountConverter.Parse(GetChildValue(trn, "TRNAMT"));
        var fitId = GetChildValue(trn, "FITID");
        if (amount is null || fitId is null) return null;

        return new BankTransaction(
            TransactionTypeConverter.Parse(GetChildValue(trn, "TRNTYPE")),
            OfxDateConverter.Parse(GetChildValue(trn, "DTPOSTED")) ?? DateTimeOffset.MinValue,
            OfxDateConverter.Parse(GetChildValue(trn, "DTUSER")),
            OfxDateConverter.Parse(GetChildValue(trn, "DTAVAIL")),
            amount.Value, fitId,
            GetChildValue(trn, "NAME"),
            GetChildValue(trn, "MEMO"),
            GetChildValue(trn, "CHECKNUM"),
            GetChildValue(trn, "REFNUM"),
            GetChildValue(trn, "PAYEEID"),
            GetChildValue(trn, "SIC"),
            GetChildValue(trn, "CURSYM"),
            OfxAmountConverter.Parse(GetChildValue(trn, "CURRATE")),
            GetChildValue(trn, "CORRECTFITID"),
            GetChildValue(trn, "CORRECTIVEACTION"),
            GetChildValue(trn, "SRVRTID")
        );
    }

    /// <summary>Parses credit card statement sections from the OFX XML element.</summary>
    private List<CreditCardStatement>? ParseCreditCardStatements(XElement ofx)
    {
        var ccMsgs = GetChild(ofx, "CREDITCARDMSGSRSV1");
        if (ccMsgs is null) return null;

        var statements = new List<CreditCardStatement>();
        foreach (var trn in GetChildren(ccMsgs, "CCSTMTTRNRS"))
        {
            var ccmtrs = GetChild(trn, "CCSTMTRS");
            if (ccmtrs is null) continue;

            var stmt = MapCreditCardStatement(ccmtrs, trn);
            if (stmt is not null)
                statements.Add(stmt);
        }
        return statements;
    }

    /// <summary>Maps a CCSTMTRS XML element to a <see cref="CreditCardStatement"/>.</summary>
    private CreditCardStatement? MapCreditCardStatement(XElement ccmtrs, XElement parent)
    {
        try
        {
            var currency = GetChildValue(ccmtrs, "CURDEF") ?? "USD";
            var acctNode = GetChild(ccmtrs, "CCACCTFROM");
            if (acctNode is null) return null;
            var account = new CreditCardAccount(GetChildValue(acctNode, "ACCTID") ?? string.Empty);

            var trnList = GetChild(ccmtrs, "BANKTRANLIST");
            var startDate = trnList is not null
                ? OfxDateConverter.Parse(GetChildValue(trnList, "DTSTART")) ?? DateTimeOffset.MinValue
                : DateTimeOffset.MinValue;
            var endDate = trnList is not null
                ? OfxDateConverter.Parse(GetChildValue(trnList, "DTEND")) ?? DateTimeOffset.MinValue
                : DateTimeOffset.MinValue;

            var transactions = new List<CreditCardTransaction>();
            if (trnList is not null)
            {
                foreach (var trn in GetChildren(trnList, "STMTTRN"))
                {
                    var amount = OfxAmountConverter.Parse(GetChildValue(trn, "TRNAMT"));
                    var fitId = GetChildValue(trn, "FITID");
                    if (amount is null || fitId is null) continue;

                    transactions.Add(new CreditCardTransaction(
                        TransactionTypeConverter.Parse(GetChildValue(trn, "TRNTYPE")),
                        OfxDateConverter.Parse(GetChildValue(trn, "DTPOSTED")) ?? DateTimeOffset.MinValue,
                        OfxDateConverter.Parse(GetChildValue(trn, "DTUSER")),
                        OfxDateConverter.Parse(GetChildValue(trn, "DTAVAIL")),
                        amount.Value, fitId,
                        GetChildValue(trn, "NAME"),
                        GetChildValue(trn, "MEMO"),
                        GetChildValue(trn, "REFNUM"),
                        GetChildValue(trn, "PAYEEID"),
                        GetChildValue(trn, "SIC"),
                        GetChildValue(trn, "CURSYM"),
                        OfxAmountConverter.Parse(GetChildValue(trn, "CURRATE"))
                    ));
                }
            }

            return new CreditCardStatement(
                currency, account,
                ParseBalanceElement(GetChild(ccmtrs, "LEDGERBAL")),
                ParseBalanceElement(GetChild(ccmtrs, "AVAILBAL")),
                startDate, endDate,
                transactions.AsReadOnly(),
                GetChildValue(ccmtrs, "MKTGINFO"),
                GetChildValue(parent, "TRNUID")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to map credit card statement from XML", ex);
            return null;
        }
    }

    /// <summary>Parses investment statement sections from the OFX XML element.</summary>
    private List<InvestmentStatement>? ParseInvestmentStatements(XElement ofx)
    {
        var invMsgs = GetChild(ofx, "INVSTMTMSGSRSV1");
        if (invMsgs is null) return null;

        var statements = new List<InvestmentStatement>();
        foreach (var trn in GetChildren(invMsgs, "INVSTMTTRNRS"))
        {
            var invmtrs = GetChild(trn, "INVSTMTRS");
            if (invmtrs is null) continue;

            try
            {
                var stmt = MapInvestmentStatement(invmtrs, trn);
                if (stmt is not null)
                    statements.Add(stmt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to map investment statement from XML", ex);
            }
        }
        return statements;
    }

    /// <summary>Maps an INVSTMTRS XML element to an <see cref="InvestmentStatement"/>.</summary>
    private InvestmentStatement? MapInvestmentStatement(XElement invmtrs, XElement parent)
    {
        var asOfDate = OfxDateConverter.Parse(GetChildValue(invmtrs, "DTASOF")) ?? DateTimeOffset.MinValue;
        var currency = GetChildValue(invmtrs, "CURDEF") ?? "USD";

        var acctNode = GetChild(invmtrs, "INVACCTFROM");
        if (acctNode is null)
        {
            _logger.LogWarning("INVACCTFROM not found in INVSTMTRS");
            return null;
        }
        var account = new InvestmentAccount(
            GetChildValue(acctNode, "BROKERID") ?? string.Empty,
            GetChildValue(acctNode, "ACCTID") ?? string.Empty
        );

        var invBal = GetChild(invmtrs, "INVBAL");
        Balance? availCash = null;
        decimal? marginBalance = null;
        decimal? shortBalance = null;
        if (invBal is not null)
        {
            var cashAmt = OfxAmountConverter.Parse(GetChildValue(invBal, "AVAILCASH"));
            if (cashAmt.HasValue) availCash = new Balance(cashAmt.Value, asOfDate);
            marginBalance = OfxAmountConverter.Parse(GetChildValue(invBal, "MARGINBALANCE"));
            shortBalance = OfxAmountConverter.Parse(GetChildValue(invBal, "SHORTBALANCE"));
        }

        var trnUid = GetChildValue(parent, "TRNUID");

        // Parse transactions
        var tranList = GetChild(invmtrs, "INVTRANLIST");
        DateTimeOffset? startDate = null, endDate = null;
        var transactions = new List<InvestmentTransaction>();

        if (tranList is not null)
        {
            startDate = OfxDateConverter.Parse(GetChildValue(tranList, "DTSTART"));
            endDate = OfxDateConverter.Parse(GetChildValue(tranList, "DTEND"));

            foreach (var category in tranList.Elements())
            {
                if (string.Equals(category.Name.LocalName, "DTSTART", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(category.Name.LocalName, "DTEND", StringComparison.OrdinalIgnoreCase))
                    continue;

                var tx = MapInvestmentTransactionFromXml(category, _logger);
                if (tx is not null)
                    transactions.Add(tx);
            }
        }

        // Parse positions
        var positions = new List<InvestmentPosition>();
        var posList = GetChild(invmtrs, "INVPOSLIST");
        if (posList is not null)
        {
            foreach (var pos in GetChildren(posList, "INVPOS"))
                positions.Add(MapPositionFromXml(pos));
        }

        // Parse securities
        var securities = new List<SecurityInfo>();
        var secList = GetChild(invmtrs, "SECLIST");
        if (secList is not null)
        {
            foreach (var secInfo in secList.Elements())
            {
                var secIdNode = GetChild(secInfo, "SECID");
                var sec = new SecurityInfo(
                    GetChildValue(secIdNode, "UNIQUEID") ?? string.Empty,
                    GetChildValue(secIdNode, "UNIQUEIDTYPE") ?? string.Empty,
                    GetChildValue(secInfo, "SECNAME"),
                    GetChildValue(secInfo, "TICKER"),
                    GetChildValue(secInfo, "FINAME"),
                    GetChildValue(secInfo, "UNITTYPE"),
                    secInfo.Name.LocalName,
                    OfxAmountConverter.Parse(GetChildValue(secInfo, "RATING"))
                );
                securities.Add(sec);
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

    /// <summary>Maps an investment transaction XML element to an <see cref="InvestmentTransaction"/>.</summary>
    private static InvestmentTransaction? MapInvestmentTransactionFromXml(XElement trn, IOfxLogger logger)
    {
        try
        {
            var invTran = GetChild(trn, "INVTRAN");
            var fitId = GetChildValue(invTran, "FITID") ?? GetChildValue(trn, "FITID");
            var datePosted = OfxDateConverter.Parse(GetChildValue(invTran, "DTPOSTED") ?? GetChildValue(trn, "DTPOSTED"));
            var dateSettle = OfxDateConverter.Parse(GetChildValue(invTran, "DTSETTLE") ?? GetChildValue(trn, "DTSETTLE"));

            if (fitId is null)
            {
                logger.LogWarning("Investment transaction missing FITID");
                return null;
            }

            var invTypeStr = trn.Name.LocalName;
            var invType = InvestmentTransactionTypeConverter.Parse(invTypeStr);
            if (invType == Models.Common.Enums.InvestmentTransactionType.Unknown)
                invType = InvestmentTransactionTypeConverter.Parse(GetChildValue(trn, "INVTRANFOR"));

            var secIdNode = GetChild(trn, "SECID");
            var secId = GetChildValue(secIdNode, "UNIQUEID");
            var secIdType = GetChildValue(secIdNode, "UNIQUEIDTYPE");

            return new InvestmentTransaction(
                fitId,
                datePosted ?? DateTimeOffset.MinValue,
                dateSettle,
                invType,
                OfxAmountConverter.Parse(GetChildValue(trn, "UNITS")),
                OfxAmountConverter.Parse(GetChildValue(trn, "UNITPRICE")),
                OfxAmountConverter.Parse(GetChildValue(trn, "TOTAL")),
                OfxAmountConverter.Parse(GetChildValue(trn, "COMMISSION")),
                OfxAmountConverter.Parse(GetChildValue(trn, "FEES")),
                OfxAmountConverter.Parse(GetChildValue(trn, "LOAD")),
                OfxAmountConverter.Parse(GetChildValue(trn, "INTEREST")),
                OfxAmountConverter.Parse(GetChildValue(trn, "GAIN")),
                secId, secIdType,
                GetChildValue(trn, "MEMO"),
                GetChildValue(trn, "CURSYM"),
                OfxAmountConverter.Parse(GetChildValue(trn, "CURRATE")),
                GetChildValue(trn, "INV401KSOURCE")
            );
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to map investment transaction from XML", ex);
            return null;
        }
    }

    /// <summary>Maps an INVPOS XML element to an <see cref="InvestmentPosition"/>.</summary>
    private static InvestmentPosition MapPositionFromXml(XElement pos)
    {
        var secIdNode = GetChild(pos, "SECID");
        return new InvestmentPosition(
            GetChildValue(secIdNode, "UNIQUEID") ?? string.Empty,
            GetChildValue(secIdNode, "UNIQUEIDTYPE") ?? string.Empty,
            GetChildValue(pos, "HELDINACCT") ?? string.Empty,
            GetChildValue(pos, "POSTYPE") ?? string.Empty,
            OfxAmountConverter.Parse(GetChildValue(pos, "UNITS")) ?? 0,
            OfxAmountConverter.Parse(GetChildValue(pos, "UNITPRICE")) ?? 0,
            OfxAmountConverter.Parse(GetChildValue(pos, "MKTVAL")) ?? 0,
            OfxDateConverter.Parse(GetChildValue(pos, "DTPRICEASOF")) ?? DateTimeOffset.MinValue,
            GetChildValue(pos, "CURSYM"),
            OfxAmountConverter.Parse(GetChildValue(pos, "CURRATE")),
            GetChildValue(pos, "MEMO")
        );
    }

    /// <summary>Parses loan statement sections from the OFX XML element.</summary>
    private List<LoanStatement>? ParseLoanStatements(XElement ofx)
    {
        var loanMsgs = GetChild(ofx, "LOANMSGSRSV1");
        if (loanMsgs is null) return null;

        var statements = new List<LoanStatement>();
        foreach (var trn in GetChildren(loanMsgs, "LOANSTMTTRNRS"))
        {
            var loanmtrs = GetChild(trn, "LOANSTMTRS");
            if (loanmtrs is null) continue;

            try
            {
                var stmt = MapLoanStatementFromXml(loanmtrs, trn);
                if (stmt is not null)
                    statements.Add(stmt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to map loan statement from XML", ex);
            }
        }
        return statements;
    }

    /// <summary>Maps a LOANSTMTRS XML element to a <see cref="LoanStatement"/>.</summary>
    private LoanStatement? MapLoanStatementFromXml(XElement loanmtrs, XElement parent)
    {
        var currency = GetChildValue(loanmtrs, "CURDEF") ?? "USD";

        var acctNode = GetChild(loanmtrs, "LOANACCTFROM");
        if (acctNode is null)
        {
            _logger.LogWarning("LOANACCTFROM not found in LOANSTMTRS");
            return null;
        }

        var loanType = GetChildValue(acctNode, "LOANTYPE")?.ToUpperInvariant() switch
        {
            "FIXED" => AccountType.Loan,
            "LINE" => AccountType.LineOfCredit,
            _ => AccountType.Loan
        };

        var account = new LoanAccount(
            GetChildValue(acctNode, "BANKID") ?? string.Empty,
            GetChildValue(acctNode, "ACCTID") ?? string.Empty,
            loanType
        );

        var principalBal = ParseBalanceElement(GetChild(loanmtrs, "PRINCIPALBAL"));
        var ledgerBal = ParseBalanceElement(GetChild(loanmtrs, "LEDGERBAL"));
        var availBal = ParseBalanceElement(GetChild(loanmtrs, "AVAILBAL"));

        var loanInfo = GetChild(loanmtrs, "LOANINFO");
        decimal? interestRate = null, interestYtd = null, nextPaymentAmount = null;
        DateTimeOffset? nextPaymentDate = null;
        if (loanInfo is not null)
        {
            interestRate = OfxAmountConverter.Parse(GetChildValue(loanInfo, "LOANINTRATE"));
            interestYtd = OfxAmountConverter.Parse(GetChildValue(loanInfo, "LOANINTYTD"));
            nextPaymentDate = OfxDateConverter.Parse(GetChildValue(loanInfo, "DTPAYMENTDUE"));
            nextPaymentAmount = OfxAmountConverter.Parse(GetChildValue(loanInfo, "PAYMENTAMT"));
        }

        var trnUid = GetChildValue(parent, "TRNUID");

        var tranList = GetChild(loanmtrs, "LOANTRANLIST");
        DateTimeOffset startDate = default, endDate = default;
        var transactions = new List<LoanTransaction>();

        if (tranList is not null)
        {
            startDate = OfxDateConverter.Parse(GetChildValue(tranList, "DTSTART")) ?? default;
            endDate = OfxDateConverter.Parse(GetChildValue(tranList, "DTEND")) ?? default;

            foreach (var trn in GetChildren(tranList, "LOANTRN"))
            {
                var amount = OfxAmountConverter.Parse(GetChildValue(trn, "TRNAMT"));
                var fitId = GetChildValue(trn, "FITID");
                var dtPosted = OfxDateConverter.Parse(GetChildValue(trn, "DTPOSTED"));
                if (amount is null || fitId is null || dtPosted is null) continue;

                transactions.Add(new LoanTransaction(
                    TransactionTypeConverter.Parse(GetChildValue(trn, "TRNTYPE")),
                    dtPosted.Value, amount.Value, fitId,
                    GetChildValue(trn, "NAME"),
                    GetChildValue(trn, "MEMO"),
                    OfxAmountConverter.Parse(GetChildValue(trn, "PRINCIPALAMT")),
                    OfxAmountConverter.Parse(GetChildValue(trn, "INTERESTAMT")),
                    OfxAmountConverter.Parse(GetChildValue(trn, "ESCROWAMT")),
                    GetChildValue(trn, "CURSYM"),
                    OfxAmountConverter.Parse(GetChildValue(trn, "CURRATE"))
                ));
            }
        }

        return new LoanStatement(
            currency, account,
            principalBal, ledgerBal, availBal,
            startDate, endDate,
            interestRate, interestYtd,
            nextPaymentDate, nextPaymentAmount,
            null,
            transactions.AsReadOnly(),
            trnUid
        );
    }

    /// <summary>Parses a balance element XML node into a <see cref="Balance"/>.</summary>
    private static Balance? ParseBalanceElement(XElement? balNode)
    {
        if (balNode is null) return null;
        var amount = OfxAmountConverter.Parse(GetChildValue(balNode, "BALAMT"));
        var asOf = OfxDateConverter.Parse(GetChildValue(balNode, "DTASOF"));
        return amount is not null ? new Balance(amount.Value, asOf ?? DateTimeOffset.MinValue) : null;
    }
}
