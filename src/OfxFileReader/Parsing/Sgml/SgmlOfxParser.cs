using OfxFileReader.Exceptions;
using OfxFileReader.Logging;
using OfxFileReader.Models;
using OfxFileReader.Models.Banking;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.CreditCard;
using OfxFileReader.Models.Investment;
using OfxFileReader.Models.Loan;
using OfxFileReader.Models.SignOn;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Mappers;

namespace OfxFileReader.Parsing.Sgml;

/// <summary>Parses SGML-format OFX content into an <see cref="OfxDocument"/>.</summary>
internal sealed class SgmlOfxParser
{
    private readonly IOfxLogger _logger;

    /// <summary>Initializes a new instance with an optional logger.</summary>
    public SgmlOfxParser(IOfxLogger? logger = null)
    {
        _logger = logger ?? NullOfxLogger.Instance;
    }

    /// <summary>Parses SGML OFX content into a structured document.</summary>
    public OfxDocument Parse(string content)
    {
        var headerResult = HeaderParser.Parse(content);
        var header = headerResult.Header;

        var tokenizer = new SgmlTokenizer(headerResult.RemainingContent);
        var tokens = tokenizer.Tokenize();

        var treeBuilder = new SgmlTreeBuilder();
        treeBuilder.Build(tokens);

        if (treeBuilder.Root is null)
            throw new OfxParseException("No content found after header");

        var ofxNodes = FindAllOfxNodes(treeBuilder.Root);
        if (ofxNodes.Count == 0)
            throw new OfxParseException("Root <OFX> tag not found");

        SignOnResponse? signOn = null;
        var allBankStatements = new List<BankStatement>();
        var allCreditCardStatements = new List<CreditCardStatement>();
        var allInvestmentStatements = new List<InvestmentStatement>();
        var allLoanStatements = new List<LoanStatement>();

        foreach (var ofxNode in ofxNodes)
        {
            signOn ??= ParseSignOn(ofxNode);

            var bankStmts = ParseBankStatements(ofxNode);
            if (bankStmts is not null)
                allBankStatements.AddRange(bankStmts);

            var ccStmts = ParseCreditCardStatements(ofxNode);
            if (ccStmts is not null)
                allCreditCardStatements.AddRange(ccStmts);

            var invStmts = ParseInvestmentStatements(ofxNode);
            if (invStmts is not null)
                allInvestmentStatements.AddRange(invStmts);

            var loanStmts = ParseLoanStatements(ofxNode);
            if (loanStmts is not null)
                allLoanStatements.AddRange(loanStmts);
        }

        var totalTransactions = allBankStatements.Sum(s => s.Transactions.Count)
            + allCreditCardStatements.Sum(s => s.Transactions.Count)
            + allInvestmentStatements.Sum(s => s.Transactions?.Count ?? 0)
            + allLoanStatements.Sum(s => s.Transactions?.Count ?? 0);

        var totalStatements = allBankStatements.Count
            + allCreditCardStatements.Count
            + allInvestmentStatements.Count
            + allLoanStatements.Count;

        var metadata = new OfxParseMetadata(
            ParsedAt: DateTimeOffset.UtcNow,
            ParserVersion: "1.0.0",
            DetectedEncoding: header.Encoding,
            TransactionCount: totalTransactions,
            StatementCount: totalStatements
        );

        return new OfxDocument(
            header,
            metadata,
            signOn,
            allBankStatements.AsReadOnly(),
            allCreditCardStatements.AsReadOnly(),
            allInvestmentStatements.AsReadOnly(),
            allLoanStatements.AsReadOnly()
        );
    }

    /// <summary>Finds all &lt;OFX&gt; nodes in the parse tree.</summary>
    private static List<SgmlNode> FindAllOfxNodes(SgmlNode root)
    {
        var result = new List<SgmlNode>();
        FindAllOfxNodesRecursive(root, result);
        return result;
    }

    /// <summary>Recursively searches for &lt;OFX&gt; nodes starting from the given node.</summary>
    private static void FindAllOfxNodesRecursive(SgmlNode node, List<SgmlNode> result)
    {
        if (string.Equals(node.Name, "OFX", StringComparison.OrdinalIgnoreCase))
        {
            result.Add(node);
            return;
        }

        foreach (var child in node.Children)
            FindAllOfxNodesRecursive(child, result);
    }

    /// <summary>Parses the sign-on response (SONRS) from the OFX node.</summary>
    private SignOnResponse? ParseSignOn(SgmlNode ofxNode)
    {
        var signonMsgs = ofxNode.FindChild("SIGNONMSGSRSV1");
        if (signonMsgs is null)
        {
            _logger.LogDebug("SIGNONMSGSRSV1 not found");
            return null;
        }

        var sonrs = signonMsgs.FindChild("SONRS");
        if (sonrs is null)
        {
            _logger.LogWarning("SONRS not found within SIGNONMSGSRSV1");
            return null;
        }

        return SignOnMapper.Map(sonrs, _logger);
    }

    /// <summary>Parses bank statements (BANKMSGSRSV1) from the OFX node.</summary>
    private List<BankStatement>? ParseBankStatements(SgmlNode ofxNode)
    {
        var bankMsgs = ofxNode.FindChild("BANKMSGSRSV1");
        if (bankMsgs is null)
            return null;

        var statements = new List<BankStatement>();
        var trnNodes = bankMsgs.FindChildren("STMTTRNRS");

        foreach (var trnNode in trnNodes)
        {
            var stmtrs = trnNode.FindChild("STMTRS");
            if (stmtrs is null)
                continue;

            var stmt = BankMapper.MapStatement(stmtrs, _logger);
            if (stmt is not null)
                statements.Add(stmt);
        }

        return statements;
    }

    /// <summary>Parses credit card statements (CREDITCARDMSGSRSV1) from the OFX node.</summary>
    private List<CreditCardStatement>? ParseCreditCardStatements(SgmlNode ofxNode)
    {
        var ccMsgs = ofxNode.FindChild("CREDITCARDMSGSRSV1");
        if (ccMsgs is null)
            return null;

        var statements = new List<CreditCardStatement>();
        var trnNodes = ccMsgs.FindChildren("CCSTMTTRNRS");

        foreach (var trnNode in trnNodes)
        {
            var ccmtrs = trnNode.FindChild("CCSTMTRS");
            if (ccmtrs is null)
                continue;

            var stmt = CreditCardMapper.MapStatement(ccmtrs, _logger);
            if (stmt is not null)
                statements.Add(stmt);
        }

        return statements;
    }

    /// <summary>Parses investment statements (INVSTMTMSGSRSV1) from the OFX node.</summary>
    private List<InvestmentStatement>? ParseInvestmentStatements(SgmlNode ofxNode)
    {
        var invMsgs = ofxNode.FindChild("INVSTMTMSGSRSV1");
        if (invMsgs is null)
            return null;

        var statements = new List<InvestmentStatement>();
        var trnNodes = invMsgs.FindChildren("INVSTMTTRNRS");

        foreach (var trnNode in trnNodes)
        {
            var invmtrs = trnNode.FindChild("INVSTMTRS");
            if (invmtrs is null)
                continue;

            var stmt = InvestmentMapper.MapStatement(invmtrs, _logger);
            if (stmt is not null)
                statements.Add(stmt);
        }

        return statements;
    }

    /// <summary>Parses loan statements (LOANMSGSRSV1) from the OFX node.</summary>
    private List<LoanStatement>? ParseLoanStatements(SgmlNode ofxNode)
    {
        var loanMsgs = ofxNode.FindChild("LOANMSGSRSV1");
        if (loanMsgs is null)
            return null;

        var statements = new List<LoanStatement>();
        var trnNodes = loanMsgs.FindChildren("LOANSTMTTRNRS");

        foreach (var trnNode in trnNodes)
        {
            var loanmtrs = trnNode.FindChild("LOANSTMTRS");
            if (loanmtrs is null)
                continue;

            var stmt = LoanMapper.MapStatement(loanmtrs, _logger);
            if (stmt is not null)
                statements.Add(stmt);
        }

        return statements;
    }
}
