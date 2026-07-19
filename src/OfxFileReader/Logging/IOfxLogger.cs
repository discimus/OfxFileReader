namespace OfxFileReader.Logging;

public interface IOfxLogger
{
    void LogWarning(string message, Exception? exception = null);
    void LogError(string message, Exception? exception = null);
    void LogInformation(string message);
    void LogDebug(string message);
}
