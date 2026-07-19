namespace OfxFileReader.Logging;

/// <summary>Defines a logging abstraction for the OFX reader.</summary>
public interface IOfxLogger
{
    /// <summary>Logs a warning message with an optional exception.</summary>
    void LogWarning(string message, Exception? exception = null);

    /// <summary>Logs an error message with an optional exception.</summary>
    void LogError(string message, Exception? exception = null);

    /// <summary>Logs an informational message.</summary>
    void LogInformation(string message);

    /// <summary>Logs a debug message.</summary>
    void LogDebug(string message);
}
