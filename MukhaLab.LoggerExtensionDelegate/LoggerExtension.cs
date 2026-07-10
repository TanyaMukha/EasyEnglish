using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace MukhaLab.LoggerExtensionDelegate;

/// <summary>
/// Provides optimized logging extension methods with enhanced browser console support.
/// </summary>
/// <remarks>
/// Every method here is built on <see cref="LoggerMessage.Define{T}(LogLevel, EventId, string)"/>
/// (and its multi-parameter overloads), the high-performance logging pattern recommended by
/// Microsoft.Extensions.Logging. The delegate returned by <c>LoggerMessage.Define</c> checks
/// <see cref="ILogger.IsEnabled(LogLevel)"/> internally and skips message-template formatting
/// entirely when the level is disabled, which avoids the boxing/allocation overhead of the
/// standard <see cref="LoggerExtensions.LogInformation(ILogger, string, object[])"/>-style calls.
/// </remarks>
public static class LoggerExtension
{
    // Core single-parameter delegates: one per level, sharing the "{Message}" template.
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

    // Additional delegates for structured logging (caller context / user context placeholders).
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
    /// Logs an information message tagged with the calling method name and a user identifier,
    /// formatted as <c>[{memberName}] User: {userId} - {message}</c>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="memberName">The calling method name (automatically provided); rendered as the "Context" in the log template.</param>
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
    /// Logs how long an operation took. Logs at <see cref="LogLevel.Warning"/> with a "(SLOW)"
    /// marker when <paramref name="elapsedMilliseconds"/> exceeds 1000 ms, otherwise logs at
    /// <see cref="LogLevel.Information"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operationName">The name of the operation.</param>
    /// <param name="elapsedMilliseconds">The elapsed time in milliseconds.</param>
    /// <remarks>
    /// Each branch is gated by <see cref="ILogger.IsEnabled(LogLevel)"/> for the level it actually
    /// logs at: the slow-operation branch checks <see cref="LogLevel.Warning"/>, the normal branch
    /// checks <see cref="LogLevel.Information"/>. A category configured to allow
    /// <see cref="LogLevel.Warning"/> while suppressing <see cref="LogLevel.Information"/> still
    /// receives slow-operation warnings.
    /// </remarks>
    public static void FastPerformanceLog(this ILogger? logger, string operationName, long elapsedMilliseconds)
    {
        if (logger == null) return;

        if (elapsedMilliseconds > 1000) // > 1 second
        {
            if (logger.IsEnabled(LogLevel.Warning))
                FastWarningLogger(logger, $"PERFORMANCE: {operationName} took {elapsedMilliseconds}ms (SLOW)", null);
        }
        else if (logger.IsEnabled(LogLevel.Information))
        {
            FastInfoLogger(logger, $"PERFORMANCE: {operationName} took {elapsedMilliseconds}ms", null);
        }
    }

    /// <summary>
    /// Logs method entry at <see cref="LogLevel.Debug"/> (useful for tracing execution flow).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="parameters">Method parameters to log.</param>
    /// <param name="memberName">The calling method name (automatically provided).</param>
    /// <remarks>
    /// <paramref name="memberName"/> is resolved by the compiler at the call site via
    /// <see cref="CallerMemberNameAttribute"/>. Calling this method indirectly from inside a helper
    /// or wrapper captures the name of that wrapper, not the name of the code that ultimately
    /// triggered the log — pass an explicit <paramref name="memberName"/> in that case if the
    /// caller's name matters (as <see cref="PerformanceLoggerExtensions.BeginTimedScope"/> does).
    /// </remarks>
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
    /// Logs method exit at <see cref="LogLevel.Debug"/> (useful for tracing execution flow).
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="result">Method result to log.</param>
    /// <param name="memberName">The calling method name (automatically provided).</param>
    /// <remarks>
    /// Same <see cref="CallerMemberNameAttribute"/> caveat as <see cref="FastMethodEntry"/> applies:
    /// calling this indirectly captures the immediate caller's name, not the original caller's.
    /// </remarks>
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

    // Simple message-logging overloads (no caller-context capture). Overloads that take an
    // already-formatted "message" string log unconditionally and rely on the internal
    // IsEnabled check inside the LoggerMessage.Define delegate; overloads that take a composite
    // "format" string check IsEnabled explicitly first, to skip the cost of string.Format when
    // the level is disabled.

    /// <summary>Logs a pre-formatted message at <see cref="LogLevel.Information"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    public static void FastInfoMessage(this ILogger? logger, string message)
    {
        if (logger != null)
        {
            FastInfoLogger(logger, message, null);
        }
    }

    /// <summary>Formats <paramref name="format"/> with <paramref name="args"/> and logs it at <see cref="LogLevel.Information"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">A composite format string (<see cref="string.Format(IFormatProvider, string, object[])"/> syntax).</param>
    /// <param name="args">Format arguments.</param>
    public static void FastInfoMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Information))
        {
            FastInfoLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>Logs a pre-formatted message and an optional exception at <see cref="LogLevel.Error"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The associated exception, if any.</param>
    public static void FastErrorMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastErrorLogger(logger, message, exception);
        }
    }

    /// <summary>Formats <paramref name="format"/> with <paramref name="args"/> and logs it at <see cref="LogLevel.Error"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">A composite format string (<see cref="string.Format(IFormatProvider, string, object[])"/> syntax).</param>
    /// <param name="args">Format arguments.</param>
    public static void FastErrorMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Error))
        {
            FastErrorLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>Formats <paramref name="format"/> with <paramref name="args"/> and logs it with <paramref name="exception"/> at <see cref="LogLevel.Error"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The associated exception.</param>
    /// <param name="format">A composite format string (<see cref="string.Format(IFormatProvider, string, object[])"/> syntax).</param>
    /// <param name="args">Format arguments.</param>
    public static void FastErrorMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Error))
        {
            FastErrorLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }

    /// <summary>Logs a pre-formatted message and an optional exception at <see cref="LogLevel.Warning"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The associated exception, if any.</param>
    public static void FastWarningMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastWarningLogger(logger, message, exception);
        }
    }

    /// <summary>Formats <paramref name="format"/> with <paramref name="args"/> and logs it at <see cref="LogLevel.Warning"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">A composite format string (<see cref="string.Format(IFormatProvider, string, object[])"/> syntax).</param>
    /// <param name="args">Format arguments.</param>
    public static void FastWarningMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Warning))
        {
            FastWarningLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>Formats <paramref name="format"/> with <paramref name="args"/> and logs it with <paramref name="exception"/> at <see cref="LogLevel.Warning"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The associated exception.</param>
    /// <param name="format">A composite format string (<see cref="string.Format(IFormatProvider, string, object[])"/> syntax).</param>
    /// <param name="args">Format arguments.</param>
    public static void FastWarningMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Warning))
        {
            FastWarningLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }

    /// <summary>Logs a pre-formatted message and an optional exception at <see cref="LogLevel.Debug"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="exception">The associated exception, if any.</param>
    public static void FastDebugMessage(this ILogger? logger, string message, Exception? exception = null)
    {
        if (logger != null)
        {
            FastDebugLogger(logger, message, exception);
        }
    }

    /// <summary>Formats <paramref name="format"/> with <paramref name="args"/> and logs it at <see cref="LogLevel.Debug"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="format">A composite format string (<see cref="string.Format(IFormatProvider, string, object[])"/> syntax).</param>
    /// <param name="args">Format arguments.</param>
    public static void FastDebugMessage(this ILogger? logger, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            FastDebugLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), null);
        }
    }

    /// <summary>Formats <paramref name="format"/> with <paramref name="args"/> and logs it with <paramref name="exception"/> at <see cref="LogLevel.Debug"/>.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The associated exception.</param>
    /// <param name="format">A composite format string (<see cref="string.Format(IFormatProvider, string, object[])"/> syntax).</param>
    /// <param name="args">Format arguments.</param>
    public static void FastDebugMessage(this ILogger? logger, Exception exception, string format, params object[] args)
    {
        if (logger != null && logger.IsEnabled(LogLevel.Debug))
        {
            FastDebugLogger(logger, string.Format(CultureInfo.CurrentCulture, format, args), exception);
        }
    }
}
