using System.Globalization;
using Microsoft.Extensions.Logging;

namespace LogerExtensionDelegate;

/// <summary>
/// Provides optimized logging extension methods.
/// </summary>
public static class LogerExtension
{
    // Define log templates for simple messages
    private static readonly Action<ILogger, string, Exception?> FastInfoLogger =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, "INFO"),
            "{Message}");

    private static readonly Action<ILogger, string, Exception?> FastErrorLogger =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            new EventId(2, "ERROR"),
            "{Message}");

    private static readonly Action<ILogger, string, Exception?> FastWarningLogger =
        LoggerMessage.Define<string>(
            LogLevel.Warning,
            new EventId(3, "WARNING"),
            "{Message}");

    private static readonly Action<ILogger, string, Exception?> FastDebugLogger =
        LoggerMessage.Define<string>(
            LogLevel.Debug,
            new EventId(4, "DEBUG"),
            "{Message}");

    /// <summary>
    /// Logs an information message with optimized performance.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    public static void FastInfoMessage(this ILogger? logger, string message)
    {
        if (logger != null)
        {
            FastInfoLogger(logger, message, null);
        }
    }

    /// <summary>
    /// Logs an information message with optimized performance, supporting format strings.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    public static void FastInfoMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Information))
        {
            FastInfoLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>
    /// Logs an error message with optimized performance.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log (optional).</param>
    public static void FastErrorMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastErrorLogger(logger, message, exception);
        }
    }

    /// <summary>
    /// Logs an error message with optimized performance, supporting format strings.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    public static void FastErrorMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Error))
        {
            FastErrorLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>
    /// Logs an error message with optimized performance, supporting format strings with exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    public static void FastErrorMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Error))
        {
            FastErrorLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }

    /// <summary>
    /// Logs a warning message with optimized performance.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log (optional).</param>
    public static void FastWarningMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastWarningLogger(logger, message, exception);
        }
    }

    /// <summary>
    /// Logs a warning message with optimized performance, supporting format strings.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    public static void FastWarningMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Warning))
        {
            FastWarningLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>
    /// Logs a warning message with optimized performance, supporting format strings with exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    public static void FastWarningMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Warning))
        {
            FastWarningLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }

    /// <summary>
    /// Logs a debug message with optimized performance.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The exception to log (optional).</param>
    public static void FastDebugMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastDebugLogger(logger, message, exception);
        }
    }

    /// <summary>
    /// Logs a debug message with optimized performance, supporting format strings.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    public static void FastDebugMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            FastDebugLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>
    /// Logs a debug message with optimized performance, supporting format strings with exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="format">The format string.</param>
    /// <param name="args">The arguments to format.</param>
    public static void FastDebugMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            FastDebugLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }
}
