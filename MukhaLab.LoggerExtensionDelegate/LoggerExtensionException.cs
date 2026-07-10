namespace MukhaLab.LoggerExtensionDelegate;

/// <summary>
/// Exception type reserved for logging-related failures raised by consumers of this library
/// (e.g. misconfigured logging setup). Follows the standard three-constructor exception pattern.
/// </summary>
/// <remarks>
/// Not currently thrown anywhere inside <c>MukhaLab.LoggerExtensionDelegate</c> itself — the
/// "Fast*" extension methods in <see cref="LogerExtension"/> and
/// <see cref="PerformanceLoggerExtensions"/> never fail on a null or misbehaving
/// <see cref="Microsoft.Extensions.Logging.ILogger"/>, they simply no-op. This type exists for
/// application code that wants a dedicated, catchable exception type for its own logging-related
/// error paths.
/// </remarks>
public class LoggerExtensionException : Exception
{
    /// <summary>Initializes a new instance with no message.</summary>
    public LoggerExtensionException()
    {
    }

    /// <summary>Initializes a new instance with the specified error message.</summary>
    /// <param name="message">The message that describes the error.</param>
    public LoggerExtensionException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with the specified error message and inner exception.</summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of this exception.</param>
    public LoggerExtensionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
