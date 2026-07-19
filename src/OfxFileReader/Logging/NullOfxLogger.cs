namespace OfxFileReader.Logging;

internal sealed class NullOfxLogger : IOfxLogger
{
    public static readonly NullOfxLogger Instance = new();

    public void LogWarning(string message, Exception? exception = null) { }
    public void LogError(string message, Exception? exception = null) { }
    public void LogInformation(string message) { }
    public void LogDebug(string message) { }
}
