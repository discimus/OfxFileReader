using OfxFileReader.Models.Common.Enums;

namespace OfxFileReader.Tests.Integration;

/// <summary>Banking scenario integration tests covering various OFX edge cases.</summary>
public class OfxReaderBankingScenarioTests
{
    private static readonly string FixturesDir = Path.Combine(AppContext.BaseDirectory, "Fixtures");

    /// <summary>Verifies that a file with all message set types is parsed completely.</summary>
    [Fact]
    public void Read_FullMultiType_ParsesAllMessageSets()
    {
        var path = Path.Combine(FixturesDir, "full_multi_type.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.SignOn);
        Assert.Equal("MULTIBANK", doc.SignOn.FinancialInstitution?.Organization);
        Assert.NotNull(doc.BankStatements);
        Assert.Single(doc.BankStatements);
        Assert.NotNull(doc.CreditCardStatements);
        Assert.Single(doc.CreditCardStatements);
        Assert.NotNull(doc.InvestmentStatements);
        Assert.Single(doc.InvestmentStatements);
        Assert.NotNull(doc.LoanStatements);
        Assert.Single(doc.LoanStatements);
        Assert.Equal(4, doc.Metadata.StatementCount);
    }

    /// <summary>Verifies that all 17 transaction types are mapped correctly.</summary>
    [Fact]
    public void Read_AllTransactionTypes_AllMappedCorrectly()
    {
        var path = Path.Combine(FixturesDir, "all_transaction_types.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Single(doc.BankStatements);

        var txns = doc.BankStatements[0].Transactions;
        Assert.Equal(17, txns.Count);
        Assert.Equal(TransactionType.Credit, txns[0].Type);
        Assert.Equal(TransactionType.Debit, txns[1].Type);
        Assert.Equal(TransactionType.Interest, txns[2].Type);
        Assert.Equal(TransactionType.Dividend, txns[3].Type);
        Assert.Equal(TransactionType.Fee, txns[4].Type);
        Assert.Equal(TransactionType.ServiceCharge, txns[5].Type);
        Assert.Equal(TransactionType.Deposit, txns[6].Type);
        Assert.Equal(TransactionType.ATM, txns[7].Type);
        Assert.Equal(TransactionType.PointOfSale, txns[8].Type);
        Assert.Equal(TransactionType.Transfer, txns[9].Type);
        Assert.Equal(TransactionType.Check, txns[10].Type);
        Assert.Equal(TransactionType.Payment, txns[11].Type);
        Assert.Equal(TransactionType.Cash, txns[12].Type);
        Assert.Equal(TransactionType.DirectDeposit, txns[13].Type);
        Assert.Equal(TransactionType.DirectDebit, txns[14].Type);
        Assert.Equal(TransactionType.RepeatPayment, txns[15].Type);
        Assert.Equal(TransactionType.Other, txns[16].Type);
    }

    /// <summary>Verifies that sign-on error status codes are detected and parsed.</summary>
    [Fact]
    public void Read_ErrorStatus_DetectsSignonError()
    {
        var path = Path.Combine(FixturesDir, "error_status.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.SignOn);
        Assert.Equal(2000, doc.SignOn.Status.Code);
        Assert.Equal(SeverityType.Error, doc.SignOn.Status.Severity);
        Assert.Equal("Invalid user ID or password", doc.SignOn.Status.Message);
    }

    /// <summary>Verifies that corrected/reversal transaction fields are parsed correctly.</summary>
    [Fact]
    public void Read_CorrectedTransactions_ParsesReversalFields()
    {
        var path = Path.Combine(FixturesDir, "corrected_transactions.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        var txns = doc.BankStatements[0].Transactions;
        Assert.Equal(2, txns.Count);

        var reversal = txns[1];
        Assert.Equal("CORR-001", reversal.FitId);
        Assert.Equal("ORIG-001", reversal.CorrectFitId);
        Assert.Equal("RER", reversal.CorrectiveAction);
    }

    /// <summary>Verifies that multi-currency transactions with exchange rates are parsed correctly.</summary>
    [Fact]
    public void Read_MultiCurrency_ParsesCurrencyFields()
    {
        var path = Path.Combine(FixturesDir, "multi_currency.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        var txns = doc.BankStatements[0].Transactions;
        Assert.Equal(2, txns.Count);

        var eurTxn = txns[0];
        Assert.Equal("EUR", eurTxn.CurrencyCode);
        Assert.Equal(0.92m, eurTxn.CurrencyRate);

        var gbpTxn = txns[1];
        Assert.Equal("GBP", gbpTxn.CurrencyCode);
        Assert.Equal(0.79m, gbpTxn.CurrencyRate);
    }

    /// <summary>Verifies that negative (overdrawn) balances are parsed correctly.</summary>
    [Fact]
    public void Read_NegativeBalance_ParsesOverdrawn()
    {
        var path = Path.Combine(FixturesDir, "overdrawn.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.NotNull(doc.BankStatements[0].LedgerBalance);
        Assert.Equal(-135.00m, doc.BankStatements[0].LedgerBalance.Amount);
    }

    /// <summary>Verifies that loan escrow amounts are parsed correctly.</summary>
    [Fact]
    public void Read_LoanEscrow_ParsesEscrowAmount()
    {
        var path = Path.Combine(FixturesDir, "loan_escrow.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.LoanStatements);
        Assert.Single(doc.LoanStatements);

        var loan = doc.LoanStatements[0];
        Assert.NotNull(loan.Transactions);
        var tx = loan.Transactions[0];
        Assert.Equal(200.00m, tx.EscrowAmount);
        Assert.Equal(1200.00m, tx.PrincipalAmount);
        Assert.Equal(450.00m, tx.InterestAmount);
        Assert.Equal(3.75m, loan.InterestRate);
        Assert.Equal(4500.00m, loan.InterestYearToDate);
    }

    /// <summary>Verifies that a file with only sign-on and no statements is handled.</summary>
    [Fact]
    public void Read_OnlySignon_NoStatements()
    {
        var path = Path.Combine(FixturesDir, "only_signon.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.SignOn);
        Assert.Empty(doc.BankStatements!);
        Assert.Empty(doc.CreditCardStatements!);
        Assert.Empty(doc.InvestmentStatements!);
        Assert.Empty(doc.LoanStatements!);
        Assert.Equal(0, doc.Metadata.StatementCount);
        Assert.Equal(0, doc.Metadata.TransactionCount);
    }

    /// <summary>Verifies that a statement without a ledger balance is handled.</summary>
    [Fact]
    public void Read_NoBalance_StatementWithoutLedger()
    {
        var path = Path.Combine(FixturesDir, "no_balance.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Null(doc.BankStatements[0].LedgerBalance);
    }

    /// <summary>Verifies that a file with only credit card data and no sign-on is handled.</summary>
    [Fact]
    public void Read_OnlyCreditCard_NoSignon()
    {
        var path = Path.Combine(FixturesDir, "only_creditcard.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.Null(doc.SignOn);
        Assert.NotNull(doc.CreditCardStatements);
        Assert.Single(doc.CreditCardStatements);
    }

    /// <summary>Verifies that an empty transaction list results in zero transactions.</summary>
    [Fact]
    public void Read_EmptyBankTranList_ZeroTransactions()
    {
        var path = Path.Combine(FixturesDir, "empty_banktranlist.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Empty(doc.BankStatements[0].Transactions);
    }

    /// <summary>Verifies that credit line accounts are mapped to LineOfCredit type.</summary>
    [Fact]
    public void Read_CreditLineAccount_MapsToLineOfCredit()
    {
        var path = Path.Combine(FixturesDir, "bank_creditline.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(AccountType.LineOfCredit, doc.BankStatements[0].Account.AccountType);
    }

    /// <summary>Verifies that available dates and user dates are parsed correctly.</summary>
    [Fact]
    public void Read_AvailableDates_ParsesCorrectly()
    {
        var path = Path.Combine(FixturesDir, "available_dates.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        var txns = doc.BankStatements[0].Transactions;
        Assert.Equal(2, txns.Count);

        Assert.NotNull(txns[0].DateAvailable);
        var dateAvail = txns[0].DateAvailable.Value;
        Assert.Equal(2024, dateAvail.Year);
        Assert.Equal(10, dateAvail.Month);
        Assert.Equal(3, dateAvail.Day);

        Assert.NotNull(txns[1].DateUser);
        var dateUser = txns[1].DateUser.Value;
        Assert.Equal(2024, dateUser.Year);
        Assert.Equal(10, dateUser.Month);
        Assert.Equal(1, dateUser.Day);
    }

    /// <summary>Verifies that money market accounts are mapped correctly.</summary>
    [Fact]
    public void Read_MoneyMarketAccount_MapsCorrectly()
    {
        var path = Path.Combine(FixturesDir, "moneymarket_account.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(AccountType.MoneyMarket, doc.BankStatements[0].Account.AccountType);
    }

    /// <summary>Verifies that duplicate FITID values do not cause data loss — both transactions are preserved.</summary>
    [Fact]
    public void Read_DuplicateFitId_BothTransactionsPreserved()
    {
        var path = Path.Combine(FixturesDir, "duplicate_fitid.ofx");
        var reader = new OfxReader();
        var doc = reader.Read(path);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
        Assert.Equal(2, doc.BankStatements[0].Transactions.Count);
        Assert.Equal("DUP-001", doc.BankStatements[0].Transactions[0].FitId);
        Assert.Equal("DUP-001", doc.BankStatements[0].Transactions[1].FitId);
    }

    /// <summary>Verifies that the async read method works with a TextReader.</summary>
    [Fact]
    public async Task ReadAsync_FromTextReader_Works()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        var content = await File.ReadAllTextAsync(path);
        using var reader = new StringReader(content);

        var ofxReader = new OfxReader();
        var doc = await ofxReader.ReadAsync(reader);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
    }

    /// <summary>Verifies that the synchronous read method works with a TextReader.</summary>
    [Fact]
    public void Read_FromTextReader_Works()
    {
        var path = Path.Combine(FixturesDir, "bank_statement_v1.ofx");
        var content = File.ReadAllText(path);
        using var reader = new StringReader(content);

        var ofxReader = new OfxReader();
        var doc = ofxReader.Read(reader);

        Assert.NotNull(doc);
        Assert.NotNull(doc.BankStatements);
    }
}
