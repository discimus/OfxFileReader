using OfxFileReader.Logging;
using OfxFileReader.Models.Banking;
using OfxFileReader.Models.Common;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Converters;
using OfxFileReader.Parsing.Mappers;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Tests.Mappers;

/// <summary>Tests for the bank statement SGML mapper.</summary>
public class BankMapperTests
{
    private static readonly IOfxLogger Logger = NullOfxLogger.Instance;

    /// <summary>Helper method to create an SGML node with an optional value.</summary>
    private static SgmlNode CreateNode(string name, string? value = null)
    {
        var node = new SgmlNode(name);
        if (value is not null)
            node.Value = value;
        return node;
    }

    /// <summary>Verifies that a complete valid bank statement SGML node maps correctly.</summary>
    [Fact]
    public void MapStatement_CompleteValid_ReturnsStatement()
    {
        var stmtrs = new SgmlNode("STMTRS");
        stmtrs.Children.Add(CreateNode("CURDEF", "USD"));

        var bankAcct = new SgmlNode("BANKACCTFROM");
        bankAcct.Children.Add(CreateNode("BANKID", "0339"));
        bankAcct.Children.Add(CreateNode("ACCTID", "123456789"));
        bankAcct.Children.Add(CreateNode("ACCTTYPE", "CHECKING"));
        stmtrs.Children.Add(bankAcct);

        var tranList = new SgmlNode("BANKTRANLIST");
        tranList.Children.Add(CreateNode("DTSTART", "20241001"));
        tranList.Children.Add(CreateNode("DTEND", "20241031"));

        var trn = new SgmlNode("STMTTRN");
        trn.Children.Add(CreateNode("TRNTYPE", "CREDIT"));
        trn.Children.Add(CreateNode("DTPOSTED", "20241015"));
        trn.Children.Add(CreateNode("TRNAMT", "1500.00"));
        trn.Children.Add(CreateNode("FITID", "FITID-001"));
        trn.Children.Add(CreateNode("NAME", "DIRECT DEPOSIT"));
        trn.Children.Add(CreateNode("MEMO", "Monthly salary"));
        tranList.Children.Add(trn);

        stmtrs.Children.Add(tranList);

        var ledger = new SgmlNode("LEDGERBAL");
        ledger.Children.Add(CreateNode("BALAMT", "5432.10"));
        ledger.Children.Add(CreateNode("DTASOF", "20241031"));
        stmtrs.Children.Add(ledger);

        var result = BankMapper.MapStatement(stmtrs, Logger);

        Assert.NotNull(result);
        Assert.Equal("USD", result.Currency);
        Assert.Equal("0339", result.Account.BankId);
        Assert.Equal("123456789", result.Account.AccountId);
        Assert.Equal(AccountType.Checking, result.Account.AccountType);
        Assert.Single(result.Transactions);
        Assert.Equal(TransactionType.Credit, result.Transactions[0].Type);
        Assert.Equal(1500.00m, result.Transactions[0].Amount);
        Assert.Equal("FITID-001", result.Transactions[0].FitId);
        Assert.Equal("DIRECT DEPOSIT", result.Transactions[0].Name);
        Assert.Equal("Monthly salary", result.Transactions[0].Memo);
        Assert.NotNull(result.LedgerBalance);
        Assert.Equal(5432.10m, result.LedgerBalance.Amount);
    }

    /// <summary>Verifies that a missing bank account node returns null.</summary>
    [Fact]
    public void MapStatement_MissingBankAcct_ReturnsNull()
    {
        var stmtrs = new SgmlNode("STMTRS");
        stmtrs.Children.Add(CreateNode("CURDEF", "USD"));
        var result = BankMapper.MapStatement(stmtrs, Logger);
        Assert.Null(result);
    }

    /// <summary>Verifies that transactions missing required fields are skipped.</summary>
    [Fact]
    public void MapStatement_MissingRequiredFields_SkipsTransaction()
    {
        var stmtrs = new SgmlNode("STMTRS");
        stmtrs.Children.Add(CreateNode("CURDEF", "USD"));
        var bankAcct = new SgmlNode("BANKACCTFROM");
        bankAcct.Children.Add(CreateNode("BANKID", "0339"));
        bankAcct.Children.Add(CreateNode("ACCTID", "123456789"));
        bankAcct.Children.Add(CreateNode("ACCTTYPE", "CHECKING"));
        stmtrs.Children.Add(bankAcct);

        var tranList = new SgmlNode("BANKTRANLIST");
        var trnNoFitId = new SgmlNode("STMTTRN");
        trnNoFitId.Children.Add(CreateNode("TRNTYPE", "CREDIT"));
        trnNoFitId.Children.Add(CreateNode("TRNAMT", "100.00"));
        tranList.Children.Add(trnNoFitId);

        var trnValid = new SgmlNode("STMTTRN");
        trnValid.Children.Add(CreateNode("TRNTYPE", "DEBIT"));
        trnValid.Children.Add(CreateNode("DTPOSTED", "20241015"));
        trnValid.Children.Add(CreateNode("TRNAMT", "-50.00"));
        trnValid.Children.Add(CreateNode("FITID", "FITID-002"));
        tranList.Children.Add(trnValid);
        stmtrs.Children.Add(tranList);

        var ledgerBal = new SgmlNode("LEDGERBAL");
        ledgerBal.Children.Add(CreateNode("BALAMT", "50.00"));
        stmtrs.Children.Add(ledgerBal);

        var result = BankMapper.MapStatement(stmtrs, Logger);

        Assert.NotNull(result);
        Assert.Single(result.Transactions);
        Assert.Equal(-50.00m, result.Transactions[0].Amount);
    }

    /// <summary>Verifies that all bank account types are mapped correctly.</summary>
    [Fact]
    public void MapAccount_AllTypes_MapsCorrectly()
    {
        Assert.Equal(AccountType.Checking, MapAccountType("CHECKING"));
        Assert.Equal(AccountType.Savings, MapAccountType("SAVINGS"));
        Assert.Equal(AccountType.MoneyMarket, MapAccountType("MONEYMRKT"));
        Assert.Equal(AccountType.LineOfCredit, MapAccountType("CREDITLINE"));
        Assert.Equal(AccountType.Unknown, MapAccountType("UNKNOWN"));
        Assert.Equal(AccountType.Unknown, MapAccountType(null));
    }

    /// <summary>Verifies that a null balance node returns null.</summary>
    [Fact]
    public void ParseBalance_NullNode_ReturnsNull()
    {
        var result = BankMapper.ParseBalance(null);
        Assert.Null(result);
    }

    /// <summary>Verifies that a balance node missing the amount field returns null.</summary>
    [Fact]
    public void ParseBalance_MissingAmount_ReturnsNull()
    {
        var bal = new SgmlNode("LEDGERBAL");
        bal.Children.Add(CreateNode("DTASOF", "20241031"));
        var result = BankMapper.ParseBalance(bal);
        Assert.Null(result);
    }

    /// <summary>Helper to map an account type string using the actual mapper logic.</summary>
    private static AccountType MapAccountType(string? acctType)
    {
        var acctNode = new SgmlNode("BANKACCTFROM");
        acctNode.Children.Add(CreateNode("BANKID", "0339"));
        acctNode.Children.Add(CreateNode("ACCTID", "123456789"));
        if (acctType is not null)
            acctNode.Children.Add(CreateNode("ACCTTYPE", acctType));

        var stmtrs = new SgmlNode("STMTRS");
        stmtrs.Children.Add(CreateNode("CURDEF", "USD"));
        stmtrs.Children.Add(acctNode);

        var result = BankMapper.MapStatement(stmtrs, Logger);
        return result?.Account.AccountType ?? AccountType.Unknown;
    }
}
