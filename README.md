# OfxFileReader

ℹ️ Disclosure: This application was developed using AI (vibecoding).

Class library .NET 8 para leitura e parsing de arquivos OFX (Open Financial Exchange).

Suporta **OFX 1.x (SGML)** e **OFX 2.x (XML)** com detecção automática de formato.

## Features

- Parse de extratos bancários (CHECKING, SAVINGS, MONEYMRKT, CREDITLINE)
- Parse de faturas de cartão de crédito
- Parse de extratos de investimento (ações, fundos, opções)
- Parse de extratos de empréstimo (com informações de escrow)
- Suporte a múltiplos blocos `<OFX>` concatenados
- Detecção automática de encoding
- Timezones (EST, GMT, UTC, offsets customizados)
- Valores monetários como `decimal` (GAAP, parênteses para negativos)
- API síncrona e assíncrona
- Imutabilidade — todos os modelos são `record`
- Integração via `IOfxReader` (DI-friendly)

## Instalação

```bash
dotnet add package OfxFileReader
```

## Uso Rápido

```csharp
using OfxFileReader;

var reader = new OfxReader();
var doc = reader.Read("extrato.ofx");

Console.WriteLine($"Conta: {doc.BankStatements[0].Account.AccountId}");
Console.WriteLine($"Saldo: {doc.BankStatements[0].LedgerBalance?.Amount:C}");

foreach (var t in doc.BankStatements[0].Transactions)
    Console.WriteLine($"{t.DatePosted:d} {t.Type,-12} {t.Amount,10:C} {t.Name}");
```

## API

### OfxReader

| Método | Descrição |
|---|---|
| `Read(string filePath)` | Lê arquivo OFX |
| `Read(Stream stream)` | Lê de stream |
| `Read(TextReader reader)` | Lê de TextReader |
| `ReadAsync(...)` | Versões assíncronas com `CancellationToken` |

### OfxReaderOptions

```csharp
new OfxReaderOptions
{
    Encoding = Encoding.UTF8,
    StrictMode = false,
    Logger = myLogger
};
```

### Modelos

| Modelo | Descrição |
|---|---|
| `OfxDocument` | Documento raiz com header, metadata e statements |
| `BankStatement` | Extrato bancário (BANKMSGSRSV1) |
| `CreditCardStatement` | Fatura cartão (CREDITCARDMSGSRSV1) |
| `InvestmentStatement` | Extrato investimento (INVSTMTMSGSRSV1) |
| `LoanStatement` | Extrato empréstimo (LOANMSGSRSV1) |
| `BankTransaction` | Transação bancária com FITID, datas, amount |
| `Status` | Código de status com severidade |

## Message Sets Suportados

- `SIGNONMSGSRSV1` — Autenticação
- `BANKMSGSRSV1` — Banking (checking, savings, money market, credit line)
- `CREDITCARDMSGSRSV1` — Cartão de crédito
- `INVSTMTMSGSRSV1` — Investimentos (ações, fundos, opções)
- `LOANMSGSRSV1` — Empréstimos (com escrow)

## Arquitetura

```
IOfxReader (Facade) -> HeaderParser -> SgmlOfxParser | XmlOfxParser -> Mappers -> OfxDocument
```

- **Strategy Pattern**: SGML vs XML detectado automaticamente
- **Chain of Responsibility**: Tokenizer -> TreeBuilder -> Mappers
- **Imutabilidade**: Todos os outputs são `record`

## Práticas Bancárias Aplicadas

- `decimal` para valores monetários (nunca `float`/`double`)
- `DateTimeOffset` com timezone preservado
- `FitId` como chave primária de deduplicação
- Parênteses GAAP para valores negativos: `(1500.00)` = -1500.00
- Idempotência via `CancellationToken` seguro
- Logging estruturado com `IOfxLogger`
- Proteção XXE no parser XML

## Limitações Conhecidas

- `StrictMode` disponível em `OfxReaderOptions` mas sem efeito atual
- Parsing de investimento via XML é limitado (recomendado usar OFX 1.x SGML)
- Message sets não implementados: INTERXFER, WIREXFER, BILLPAY, EMAIL, TAX

## Licença

MIT
