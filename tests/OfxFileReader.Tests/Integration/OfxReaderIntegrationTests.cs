using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Tests.Integration;

public class OfxReaderIntegrationTests
{
    private static readonly string FixturesDir = Path.Combine(AppContext.BaseDirectory, "Fixtures");
    private readonly OfxReader _reader = new();

    [Fact]
    public void Read_BankStatementV1_ParsesCorrectly()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        var doc = _reader.Read(path);

        Assert.NotNull(doc);
        Assert.Equal(OfxVersion.V1x, doc.Header.Version);
        Assert.NotNull(doc.SignOn);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(2, doc.BankStatements.Count);

        // First account: checking
        var checking = doc.BankStatements[0];
        Assert.Equal("0339", checking.Account.BankId);
        Assert.Equal("123456789", checking.Account.AccountId);
        Assert.Equal(AccountType.Checking, checking.Account.AccountType);
        Assert.Equal(5, checking.Transactions.Count);
        Assert.Equal(5432.10m, checking.LedgerBalance!.Amount);

        // Verify first transaction
        var firstTx = checking.Transactions[0];
        Assert.Equal(TransactionType.Credit, firstTx.Type);
        Assert.Equal(1500.00m, firstTx.Amount);
        Assert.Equal("DIRECT DEPOSIT", firstTx.Name);
        Assert.Equal("202410020001", firstTx.FitId);

        // Second transaction (debit)
        var debitTx = checking.Transactions[1];
        Assert.Equal(TransactionType.Debit, debitTx.Type);
        Assert.Equal(-45.50m, debitTx.Amount);
        Assert.Equal("AMAZON.COM", debitTx.Name);

        // Second account: savings
        var savings = doc.BankStatements[1];
        Assert.Equal("987654321", savings.Account.AccountId);
        Assert.Equal(AccountType.Savings, savings.Account.AccountType);
        Assert.Equal(2, savings.Transactions.Count);
    }

    [Fact]
    public void Read_BankStatementV2_ParsesCorrectly()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v2.ofx");
        var doc = _reader.Read(path);

        Assert.NotNull(doc);
        Assert.Equal(OfxVersion.V2x, doc.Header.Version);
        Assert.NotNull(doc.SignOn);
        Assert.NotNull(doc.BankStatements);
        Assert.Single(doc.BankStatements);

        var stmt = doc.BankStatements[0];
        Assert.Equal("0339", stmt.Account.BankId);
        Assert.Equal("555666777", stmt.Account.AccountId);
        Assert.Equal(2, stmt.Transactions.Count);

        // Check debit transaction
        var debit = stmt.Transactions[1];
        Assert.Equal(TransactionType.Debit, debit.Type);
        Assert.Equal(-75.25m, debit.Amount);
    }

    [Fact]
    public void Read_CreditCardV1_ParsesCorrectly()
    {
        var path = Path.Combine(FixturesDir, "credit_card_v1.ofx");
        var doc = _reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.CreditCardStatements);
        Assert.Single(doc.CreditCardStatements);

        var stmt = doc.CreditCardStatements[0];
        Assert.Equal("4111111111111111", stmt.Account.AccountId);
        Assert.Equal(3, stmt.Transactions.Count);

        // Mixed transaction types
        Assert.Equal(TransactionType.PointOfSale, stmt.Transactions[0].Type);
        Assert.Equal(TransactionType.Debit, stmt.Transactions[1].Type);
        Assert.Equal(TransactionType.Payment, stmt.Transactions[2].Type);
    }

    [Fact]
    public void Read_InvalidEmptyFile_ThrowsException()
    {
        var path = Path.Combine(FixturesDir, "invalid_empty.ofx");
        Assert.Throws<InvalidOfxHeaderException>(() => _reader.Read(path));
    }

    [Fact]
    public void Read_MultiAccount_ParsesAllTypes()
    {
        var path = Path.Combine(FixturesDir, "multi_account.ofx");
        var doc = _reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.NotNull(doc.CreditCardStatements);
        Assert.Single(doc.BankStatements);
        Assert.Single(doc.CreditCardStatements);

        // Bank account in BRL
        var bank = doc.BankStatements[0];
        Assert.Equal("BRL", bank.Currency);
        Assert.Equal("111222333", bank.Account.AccountId);

        // Credit card in BRL
        var cc = doc.CreditCardStatements[0];
        Assert.Equal("BRL", cc.Currency);
        Assert.Equal("5222222222222222", cc.Account.AccountId);
    }

    [Fact]
    public void Read_InvestmentV2_ParsesCorrectly()
    {
        var path = Path.Combine(FixturesDir, "investment_v2.ofx");
        var doc = _reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.InvestmentStatements);
        Assert.Single(doc.InvestmentStatements);

        var inv = doc.InvestmentStatements[0];
        Assert.Equal("BROKER01", inv.Account.BrokerId);
        Assert.Equal("888999000", inv.Account.AccountId);
    }

    [Fact]
    public void Read_LoanV1_ParsesCorrectly()
    {
        var path = Path.Combine(FixturesDir, "loan_v1.ofx");
        var doc = _reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.LoanStatements);
        Assert.Single(doc.LoanStatements);

        var loan = doc.LoanStatements[0];
        Assert.Equal("0339", loan.Account.BankId);
        Assert.Equal("777888999", loan.Account.AccountId);
        Assert.NotNull(loan.Transactions);
        Assert.Single(loan.Transactions);

        var tx = loan.Transactions[0];
        Assert.Equal(TransactionType.Debit, tx.Type);
        Assert.Equal(-850.00m, tx.Amount);
        Assert.Equal(650.00m, tx.PrincipalAmount);
        Assert.Equal(200.00m, tx.InterestAmount);
    }

    [Fact]
    public async Task ReadAsync_WithCancellation_WorksCorrectly()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        var cts = new CancellationTokenSource();

        var doc = await _reader.ReadAsync(path, cts.Token);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
    }

    [Fact]
    public void Read_FromStream_ParsesCorrectly()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        using var stream = File.OpenRead(path);

        var doc = _reader.Read(stream);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(2, doc.BankStatements.Count);
    }

    [Fact]
    public void Read_FileNotFound_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => _reader.Read("non_existent_file.ofx"));
    }
}
