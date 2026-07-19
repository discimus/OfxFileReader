using System.Xml.Linq;
using OfxFileReader.Exceptions;
using OfxFileReader.Parsing.Xml;

namespace OfxFileReader.Tests.Integration;

/// <summary>Security-focused tests for the OFX reader (XXE, DoS, input validation).</summary>
public class OfxReaderSecurityTests
{
    /// <summary>Verifies that XML external entity (XXE) injection is rejected.</summary>
    [Fact]
    public void XmlParser_RejectsDtdEntities_Safe()
    {
        // Attempt XXE injection
        var maliciousContent = @"<?xml version=""1.0""?>
<!DOCTYPE foo [
  <!ENTITY xxe SYSTEM ""file:///etc/passwd"">
]>
<OFX>
  <SIGNONMSGSRSV1>
    <SONRS>
      <DTSERVER>&xxe;</DTSERVER>
    </SONRS>
  </SIGNONMSGSRSV1>
</OFX>";

        var parser = new XmlOfxParser();
        Assert.Throws<OfxParseException>(() => parser.Parse(maliciousContent));
    }

    /// <summary>Verifies that the billion laughs XML DoS attack is rejected.</summary>
    [Fact]
    public void XmlParser_RejectsBillionLaughs_Safe()
    {
        var maliciousContent = @"<?xml version=""1.0""?>
<!DOCTYPE lolz [
  <!ENTITY lol ""lol"">
  <!ENTITY lol2 ""&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;"">
  <!ENTITY lol3 ""&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;"">
  <!ENTITY lol4 ""&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;&lol3;"">
]>
<OFX>
  <SIGNONMSGSRSV1>
    <SONRS>
      <DTSERVER>20241015</DTSERVER>
    </SONRS>
  </SIGNONMSGSRSV1>
</OFX>";

        var parser = new XmlOfxParser();
        Assert.Throws<OfxParseException>(() => parser.Parse(maliciousContent));
    }

    /// <summary>Verifies that reading a non-existent file path throws <see cref="FileNotFoundException"/>.</summary>
    [Fact]
    public void Read_NonExistentFile_ThrowsFileNotFoundException()
    {
        var reader = new OfxReader();
        Assert.Throws<FileNotFoundException>(() => reader.Read("Z:\\nonexistent\\file.ofx"));
    }

    /// <summary>Verifies that passing a null stream throws <see cref="ArgumentNullException"/>.</summary>
    [Fact]
    public void Read_NullStream_ThrowsArgumentNullException()
    {
        var reader = new OfxReader();
        Assert.Throws<ArgumentNullException>(() => reader.Read((Stream)null!));
    }

    /// <summary>Verifies that passing a null TextReader throws <see cref="ArgumentNullException"/>.</summary>
    [Fact]
    public void Read_NullTextReader_ThrowsArgumentNullException()
    {
        var reader = new OfxReader();
        Assert.Throws<ArgumentNullException>(() => reader.Read((TextReader)null!));
    }

    /// <summary>Verifies that passing a null stream to the async method throws <see cref="ArgumentNullException"/>.</summary>
    [Fact]
    public void ReadAsync_NullStream_ThrowsArgumentNullException()
    {
        var reader = new OfxReader();
        Assert.ThrowsAsync<ArgumentNullException>(() => reader.ReadAsync((Stream)null!)).GetAwaiter().GetResult();
    }

    /// <summary>Verifies that a cancelled token throws <see cref="OperationCanceledException"/>.</summary>
    [Fact]
    public async Task ReadAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "bank_statement_v1.ofx");
        var reader = new OfxReader();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await reader.ReadAsync(path, cts.Token);
        });
    }
}
