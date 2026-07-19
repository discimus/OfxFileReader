using OfxFileReader.Logging;
using OfxFileReader.Models.Common.Enums;
using OfxFileReader.Parsing.Mappers;
using OfxFileReader.Parsing.Sgml;

namespace OfxFileReader.Tests.Mappers;

public class LoanMapperTests
{
    private static readonly IOfxLogger Logger = NullOfxLogger.Instance;

    private static SgmlNode CreateNode(string name, string? value = null)
    {
        var node = new SgmlNode(name);
        if (value is not null)
            node.Value = value;
        return node;
    }

    [Fact]
    public void MapStatement_CompleteValid_ReturnsStatement()
    {
        var loanmtrs = new SgmlNode("LOANSTMTRS");
        loanmtrs.Children.Add(CreateNode("CURDEF", "USD"));

        var acct = new SgmlNode("LOANACCTFROM");
        acct.Children.Add(CreateNode("BANKID", "0339"));
        acct.Children.Add(CreateNode("ACCTID", "777888999"));
        acct.Children.Add(CreateNode("LOANTYPE", "FIXED"));
        loanmtrs.Children.Add(acct);

        var tranList = new SgmlNode("LOANTRANLIST");
        tranList.Children.Add(CreateNode("DTSTART", "20241001"));
        tranList.Children.Add(CreateNode("DTEND", "20241031"));

        var trn = new SgmlNode("LOANTRN");
        trn.Children.Add(CreateNode("TRNTYPE", "DEBIT"));
        trn.Children.Add(CreateNode("DTPOSTED", "20241001"));
        trn.Children.Add(CreateNode("TRNAMT", "-850.00"));
        trn.Children.Add(CreateNode("FITID", "LOAN-001"));
        trn.Children.Add(CreateNode("NAME", "MONTHLY PAYMENT"));
        trn.Children.Add(CreateNode("PRINCIPALAMT", "650.00"));
        trn.Children.Add(CreateNode("INTERESTAMT", "200.00"));
        tranList.Children.Add(trn);
        loanmtrs.Children.Add(tranList);

        var princBal = new SgmlNode("PRINCIPALBAL");
        princBal.Children.Add(CreateNode("BALAMT", "85000.00"));
        princBal.Children.Add(CreateNode("DTASOF", "20241031"));
        loanmtrs.Children.Add(princBal);

        var loanInfo = new SgmlNode("LOANINFO");
        loanInfo.Children.Add(CreateNode("LOANINTRATE", "4.5"));
        loanInfo.Children.Add(CreateNode("LOANINTYTD", "3000.00"));
        loanmtrs.Children.Add(loanInfo);

        var parent = new SgmlNode("LOANSTMTTRNRS");
        parent.Children.Add(loanmtrs);

        var result = LoanMapper.MapStatement(loanmtrs, Logger);

        Assert.NotNull(result);
        Assert.Equal("0339", result.Account.BankId);
        Assert.Equal("777888999", result.Account.AccountId);
        Assert.Equal(AccountType.Loan, result.Account.LoanType);
        Assert.NotNull(result.Transactions);
        Assert.Single(result.Transactions);
        Assert.Equal(-850.00m, result.Transactions[0].Amount);
        Assert.Equal(650.00m, result.Transactions[0].PrincipalAmount);
        Assert.Equal(200.00m, result.Transactions[0].InterestAmount);
        Assert.NotNull(result.PrincipalBalance);
        Assert.Equal(85000.00m, result.PrincipalBalance.Amount);
        Assert.Equal(4.5m, result.InterestRate);
        Assert.Equal(3000.00m, result.InterestYearToDate);
    }

    [Fact]
    public void MapStatement_MissingAccount_ReturnsNull()
    {
        var loanmtrs = new SgmlNode("LOANSTMTRS");
        loanmtrs.Children.Add(CreateNode("CURDEF", "USD"));
        var result = LoanMapper.MapStatement(loanmtrs, Logger);
        Assert.Null(result);
    }

    [Fact]
    public void MapStatement_MissingLoanInfo_DoesNotCrash()
    {
        var loanmtrs = new SgmlNode("LOANSTMTRS");
        loanmtrs.Children.Add(CreateNode("CURDEF", "USD"));

        var acct = new SgmlNode("LOANACCTFROM");
        acct.Children.Add(CreateNode("BANKID", "0339"));
        acct.Children.Add(CreateNode("ACCTID", "777888999"));
        acct.Children.Add(CreateNode("LOANTYPE", "FIXED"));
        loanmtrs.Children.Add(acct);

        var result = LoanMapper.MapStatement(loanmtrs, Logger);

        Assert.NotNull(result);
        Assert.Null(result.InterestRate);
        Assert.Null(result.InterestYearToDate);
    }

    [Fact]
    public void MapAccount_LoanTypes_MapsCorrectly()
    {
        Assert.Equal(AccountType.Loan, MapLoanType("FIXED"));
        Assert.Equal(AccountType.LineOfCredit, MapLoanType("LINE"));
        Assert.Equal(AccountType.Loan, MapLoanType("VLN"));
        Assert.Equal(AccountType.Loan, MapLoanType("UNKNOWN"));
    }

    private static AccountType MapLoanType(string loanType)
    {
        var acct = new SgmlNode("LOANACCTFROM");
        acct.Children.Add(CreateNode("BANKID", "0339"));
        acct.Children.Add(CreateNode("ACCTID", "777888999"));
        acct.Children.Add(CreateNode("LOANTYPE", loanType));

        var loanmtrs = new SgmlNode("LOANSTMTRS");
        loanmtrs.Children.Add(CreateNode("CURDEF", "USD"));
        loanmtrs.Children.Add(acct);

        var result = LoanMapper.MapStatement(loanmtrs, Logger);
        return result?.Account.LoanType ?? AccountType.Unknown;
    }

    [Fact]
    public void MapTransaction_MissingRequiredFields_SkipsTransaction()
    {
        var loanmtrs = new SgmlNode("LOANSTMTRS");
        loanmtrs.Children.Add(CreateNode("CURDEF", "USD"));

        var acct = new SgmlNode("LOANACCTFROM");
        acct.Children.Add(CreateNode("BANKID", "0339"));
        acct.Children.Add(CreateNode("ACCTID", "777888999"));
        acct.Children.Add(CreateNode("LOANTYPE", "FIXED"));
        loanmtrs.Children.Add(acct);

        var tranList = new SgmlNode("LOANTRANLIST");
        var invalid = new SgmlNode("LOANTRN");
        invalid.Children.Add(CreateNode("TRNTYPE", "DEBIT"));
        // No FITID — should be skipped
        tranList.Children.Add(invalid);

        var valid = new SgmlNode("LOANTRN");
        valid.Children.Add(CreateNode("TRNTYPE", "DEBIT"));
        valid.Children.Add(CreateNode("DTPOSTED", "20241001"));
        valid.Children.Add(CreateNode("TRNAMT", "-100.00"));
        valid.Children.Add(CreateNode("FITID", "LOAN-002"));
        tranList.Children.Add(valid);
        loanmtrs.Children.Add(tranList);

        var result = LoanMapper.MapStatement(loanmtrs, Logger);

        Assert.NotNull(result);
        Assert.NotNull(result.Transactions);
        Assert.Single(result.Transactions);
    }
}
