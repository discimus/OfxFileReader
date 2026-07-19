namespace OfxFileReader.Logging;

/// <summary>A no-op implementation of <see cref="IOfxLogger"/> that discards all log messages.</summary>
internal sealed class NullOfxLogger : IOfxLogger
{
    /// <summary>Gets the singleton instance of the null logger.</summary>
    public static readonly NullOfxLogger Instance = new();

    /// <summary>Discards the warning message.</summary>
    public void LogWarning(string message, Exception? exception = null) { }

    /// <summary>Discards the error message.</summary>
    public void LogError(string message, Exception? exception = null) { }

    /// <summary>Discards the informational message.</summary>
    public void LogInformation(string message) { }

    /// <summary>Discards the debug message.</summary>
    public void LogDebug(string message) { }
}
