using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace MukhaLab.LoggerExtensionDelegate;

/// <summary>
/// Provides optimized logging extension methods with enhanced browser console support.
/// </summary>
public static class LogerExtension
{
    // Existing optimized delegates
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

    // Додаткові делегати для structured logging
    private static readonly Action<ILogger, string, string, Exception?> FastInfoWithContextLogger =
        LoggerMessage.Define<string, string>(
            LogLevel.Information,
            new EventId(5, "INFO_CONTEXT"),
            "[{Context}] {Message}");

    private static readonly Action<ILogger, string, string, string, Exception?> FastInfoWithUserContextLogger =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Information,
            new EventId(6, "INFO_USER_CONTEXT"),
            "[{Context}] User: {UserId} - {Message}");

    /// <summary>
    /// Logs an information message with method context (optimized for debugging).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="memberName">The calling method name (automatically provided).</param>
    /// <param name="sourceFilePath">The source file path (automatically provided).</param>
    /// <param name="sourceLineNumber">The source line number (automatically provided).</param>
    public static void FastInfoWithContext(this ILogger? logger,
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Information))
        {
            var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);
            var context = $"{fileName}.{memberName}:{sourceLineNumber}";
            FastInfoWithContextLogger(logger, context, message, null);
        }
    }

    /// <summary>
    /// Logs an information message with user context.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="memberName">The calling method name (automatically provided).</param>
    public static void FastInfoWithUserContext(this ILogger? logger,
        string userId,
        string message,
        [CallerMemberName] string memberName = "")
    {
        if (logger != null && logger.IsEnabled(LogLevel.Information))
        {
            FastInfoWithUserContextLogger(logger, memberName, userId, message, null);
        }
    }

    /// <summary>
    /// Logs performance metrics.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    public static void FastPerformanceLog(this ILogger? logger, string operationName, long elapsedMilliseconds)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Information))
        {
            if (elapsedMilliseconds > 1000) // > 1 second
            {
                FastWarningLogger(logger, $"PERFORMANCE: {operationName} took {elapsedMilliseconds}ms (SLOW)", null);
            }
            else
            {
                FastInfoLogger(logger, $"PERFORMANCE: {operationName} took {elapsedMilliseconds}ms", null);
            }
        }
    }

    /// <summary>
    /// Logs method entry (useful for debugging).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="parameters">Method parameters to log.</param>
    /// <param name="memberName">The calling method name (automatically provided).</param>
    public static void FastMethodEntry(this ILogger? logger,
        object? parameters = null,
        [CallerMemberName] string memberName = "")
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            var message = parameters != null
                ? $"ENTER {memberName}({parameters})"
                : $"ENTER {memberName}()";
            FastDebugLogger(logger, message, null);
        }
    }

    /// <summary>
    /// Logs method exit (useful for debugging).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="result">Method result to log.</param>
    /// <param name="memberName">The calling method name (automatically provided).</param>
    public static void FastMethodExit(this ILogger? logger,
        object? result = null,
        [CallerMemberName] string memberName = "")
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            var message = result != null
                ? $"EXIT {memberName} -> {result}"
                : $"EXIT {memberName}";
            FastDebugLogger(logger, message, null);
        }
    }

    // Зберігаємо всі ваші оригінальні методи
    public static void FastInfoMessage(this ILogger? logger, string message)
    {
        if (logger != null)
        {
            FastInfoLogger(logger, message, null);
        }
    }

    public static void FastInfoMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Information))
        {
            FastInfoLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    public static void FastErrorMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastErrorLogger(logger, message, exception);
        }
    }

    public static void FastErrorMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Error))
        {
            FastErrorLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    public static void FastErrorMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Error))
        {
            FastErrorLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }

    public static void FastWarningMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastWarningLogger(logger, message, exception);
        }
    }

    public static void FastWarningMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Warning))
        {
            FastWarningLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    public static void FastWarningMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Warning))
        {
            FastWarningLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }

    public static void FastDebugMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastDebugLogger(logger, message, exception);
        }
    }

    public static void FastDebugMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            FastDebugLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    public static void FastDebugMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            FastDebugLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }
}
