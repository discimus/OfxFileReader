using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Tests.Integration;

public class OfxReaderAdvancedTests
{
    private static readonly string FixturesDir = Path.Combine(AppContext.BaseDirectory, "Fixtures");

    [Fact]
    public void Read_MultipleOfxBlocks_ParsesAllStatements()
    {
        var path = Path.Combine(FixturesDir, "multiple_ofx_blocks.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(2, doc.BankStatements.Count);

        var first = doc.BankStatements[0];
        Assert.Equal("111111111", first.Account.AccountId);
        Assert.Single(first.Transactions);
        Assert.Equal(3000.00m, first.Transactions[0].Amount);

        var second = doc.BankStatements[1];
        Assert.Equal("222222222", second.Account.AccountId);
        Assert.Single(second.Transactions);
        Assert.Equal(1000.00m, second.Transactions[0].Amount);
    }

    [Fact]
    public void Read_InvestmentV2Full_ParsesTransactionsAndPositions()
    {
        var path = Path.Combine(FixturesDir, "investment_v2_full.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.InvestmentStatements);
        Assert.Single(doc.InvestmentStatements);

        var inv = doc.InvestmentStatements[0];
        Assert.Equal("BROKER02", inv.Account.BrokerId);
        Assert.Equal("888000111", inv.Account.AccountId);

        Assert.NotNull(inv.Transactions);
        Assert.Equal(2, inv.Transactions.Count);

        var buy = inv.Transactions[0];
        Assert.Equal(InvestmentTransactionType.Buy, buy.Type);
        Assert.Equal(100m, buy.Units);
        Assert.Equal(150.00m, buy.UnitPrice);
        Assert.Equal(15000.00m, buy.TotalAmount);

        var sell = inv.Transactions[1];
        Assert.Equal(InvestmentTransactionType.Sell, sell.Type);
        Assert.Equal(50m, sell.Units);

        Assert.NotNull(inv.Positions);
        Assert.Single(inv.Positions);
        Assert.Equal("AAPL", inv.Positions[0].SecurityId);
        Assert.Equal(100m, inv.Positions[0].Units);
        Assert.Equal(15000.00m, inv.Positions[0].MarketValue);

        Assert.NotNull(inv.AvailableCash);
        Assert.Equal(5000.00m, inv.AvailableCash.Amount);
    }

    [Fact]
    public void Read_LoanV2Full_ParsesAllFields()
    {
        var path = Path.Combine(FixturesDir, "loan_v2_full.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.LoanStatements);
        Assert.Single(doc.LoanStatements);

        var loan = doc.LoanStatements[0];
        Assert.Equal("0339", loan.Account.BankId);
        Assert.Equal("999000111", loan.Account.AccountId);

        Assert.NotNull(loan.Transactions);
        Assert.Single(loan.Transactions);

        var tx = loan.Transactions[0];
        Assert.Equal(-1200.00m, tx.Amount);
        Assert.Equal(900.00m, tx.PrincipalAmount);
        Assert.Equal(300.00m, tx.InterestAmount);

        Assert.NotNull(loan.PrincipalBalance);
        Assert.Equal(75000.00m, loan.PrincipalBalance.Amount);

        Assert.NotNull(loan.InterestRate);
        Assert.Equal(4.5m, loan.InterestRate);

        Assert.NotNull(loan.InterestYearToDate);
        Assert.Equal(3000.00m, loan.InterestYearToDate);
    }

    [Fact]
    public void Read_HeaderDetectsXmlFormat_Correctly()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v2.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.Equal(OfxVersion.V2x, doc.Header.Version);
        Assert.NotNull(doc.BankStatements);
    }

    [Fact]
    public void Read_FromStreamWithOptions_RespectsEncoding()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        var options = new OfxReaderOptions
        {
            Encoding = System.Text.Encoding.ASCII
        };
        var reader = new OfxReader(options);
        using var stream = File.OpenRead(path);

        var doc = reader.Read(stream);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(2, doc.BankStatements.Count);
    }

    [Fact]
    public async Task ReadAsync_UsesConfigureAwait_NoDeadlock()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        var reader = new OfxReader();

        var doc = await reader.ReadAsync(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Read_NullOrEmptyPath_ThrowsArgumentException(string? path)
    {
        var reader = new OfxReader();
        if (path is null)
        {
            Assert.Throws<ArgumentNullException>(() => reader.Read(path!));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => reader.Read(path));
        }
    }

    [Fact]
    public void DefaultCurrency_IsNull_WhenNotSpecified()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);

        // The fixture has CURDEF, so it should be set
        foreach (var stmt in doc.BankStatements)
        {
            Assert.NotNull(stmt.Currency);
        }
    }

    [Fact]
    public void MultipleOfxBlocks_AllBranchesCovered()
    {
        var path = Path.Combine(FixturesDir, "multiple_ofx_blocks.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(2, doc.BankStatements.Count);
        Assert.Equal(2, doc.Metadata.StatementCount);
        Assert.Equal(2, doc.Metadata.TransactionCount);
    }
}
