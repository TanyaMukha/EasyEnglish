# MukhaLab.LoggerExtensionDelegate

A small set of `ILogger` extension methods built on the
[`LoggerMessage.Define`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggermessage)
high-performance logging pattern. It gives call sites short, allocation-friendly "Fast*" logging
helpers instead of the standard `LogInformation`/`LogError`/... extension methods, plus a small
`using`-scoped helper for timing operations.

- **Target framework:** `net9.0`
- **Namespace:** `MukhaLab.LoggerExtensionDelegate`
- **Dependencies:** `Microsoft.Extensions.Logging` (9.0.7)
- **Consumers in this solution:** none yet — the library is currently self-contained and not
  referenced by other projects (verified by searching the solution for its public symbols).

## Table of contents

- [Why "Fast"?](#why-fast)
- [Project layout](#project-layout)
- [Installation](#installation)
- [Basic message logging](#basic-message-logging)
- [Context-aware logging](#context-aware-logging)
- [Method entry / exit tracing](#method-entry--exit-tracing)
- [Performance logging](#performance-logging)
- [Timed scopes (`BeginTimedScope`)](#timed-scopes-begintimedscope)
- [`LoggerExtensionException`](#loggerextensionexception)
- [Known limitations & gotchas](#known-limitations--gotchas)
- [Troubleshooting](#troubleshooting)

## Why "Fast"?

Standard `ILogger.LogInformation("...", args)` calls box value-type arguments and re-parse the
message template on every call. `LoggerMessage.Define<T...>(...)` compiles the template once into
a reusable, strongly-typed delegate that:

- checks `logger.IsEnabled(level)` internally before doing any work, and
- writes structured fields (`{Message}`, `{Context}`, `{UserId}`, ...) without boxing.

`LoggerExtension` wraps six such compiled delegates (`Information` / `Error` / `Warning` / `Debug`,
plus two structured "with context" variants) behind convenient extension methods, so call sites
look like ordinary logger calls:

```csharp
logger.FastInfoMessage("Import finished");
logger.FastErrorMessage(ex, "Import failed for unit {0}", unitId);
```

## Project layout

```
MukhaLab.LoggerExtensionDelegate/
├── LoggerExtension.cs             # static class LoggerExtension — compiled delegates + Fast* extension methods
├── PerformanceLoggerExtensions.cs # BeginTimedScope(...) — IDisposable operation timer
└── LoggerExtensionException.cs    # reserved exception type for consumer use (not thrown internally)
```

## Installation

Add a project reference and import the namespace — all methods are extension methods on
`ILogger`/`ILogger?`, so no DI registration is required:

```csharp
using MukhaLab.LoggerExtensionDelegate;
```

## Basic message logging

Four levels, each with the same three overload shapes:

```csharp
logger.FastInfoMessage("Cache warmed up");                                  // pre-formatted message
logger.FastInfoMessage("Loaded {0} words in {1} ms", wordCount, elapsedMs); // composite format + args
logger.FastErrorMessage(ex, "Failed to save word {0}", wordId);             // exception + format + args
```

Available per level:

| Level | Overloads |
|---|---|
| `FastInfoMessage` | `(message)`, `(format, params args)` |
| `FastErrorMessage` | `(message, exception? = null)`, `(format, params args)`, `(exception, format, params args)` |
| `FastWarningMessage` | `(message, exception? = null)`, `(format, params args)`, `(exception, format, params args)` |
| `FastDebugMessage` | `(message, exception? = null)`, `(format, params args)`, `(exception, format, params args)` |

Format strings use `string.Format(CultureInfo.CurrentCulture, format, args)` — **not** the
`{PropertyName}` structured-logging syntax used by `ILogger.LogInformation`. Use numbered
placeholders (`{0}`, `{1}`, ...).

All extension methods accept a nullable `ILogger?` and silently no-op when the logger is `null`,
so they are safe to call on an optional/not-yet-configured logger without a null check at the call
site.

**Design pattern to note:** overloads that take an already-formatted `message` string call the
underlying compiled delegate unconditionally (the delegate itself checks `IsEnabled`); overloads
that take a `format` + `args` pair check `IsEnabled` explicitly *before* calling `string.Format`,
to avoid paying for formatting when the level is disabled. Follow the same pattern if you add new
overloads.

## Context-aware logging

Two structured helpers that capture caller information automatically via
`[CallerMemberName]`/`[CallerFilePath]`/`[CallerLineNumber]`:

```csharp
logger.FastInfoWithContext("Starting import");
// -> [WordImporter.ImportAsync:42] Starting import

logger.FastInfoWithUserContext(userId: currentUser.Id, message: "Profile updated");
// -> [UpdateProfileAsync] User: <userId> - Profile updated
```

- `FastInfoWithContext` builds its context as `"{FileName}.{MemberName}:{LineNumber}"` and logs at
  `LogLevel.Information`.
- `FastInfoWithUserContext` uses only the member name as context (no file/line) and logs at
  `LogLevel.Information`.

Both are gated by `IsEnabled(LogLevel.Information)` before doing any work.

## Method entry / exit tracing

```csharp
public async Task ImportWordsAsync(int unitId)
{
    logger.FastMethodEntry(unitId);   // "ENTER ImportWordsAsync(<unitId>)"
    try
    {
        ...
        logger.FastMethodExit(importedCount); // "EXIT ImportWordsAsync -> <importedCount>"
    }
    catch { logger.FastMethodExit(); throw; }  // "EXIT ImportWordsAsync"
}
```

Both log at `LogLevel.Debug` and rely on `[CallerMemberName]` to name the method automatically.
**Call them directly from the method you want to trace** — if you call them from inside a helper
or wrapper, the captured name will be the wrapper's, not the original method's (see
[Known limitations](#known-limitations--gotchas)).

## Performance logging

```csharp
var sw = Stopwatch.StartNew();
DoWork();
logger.FastPerformanceLog(nameof(DoWork), sw.ElapsedMilliseconds);
// < 1000 ms -> Information: "PERFORMANCE: DoWork took 213ms"
// >= 1000 ms -> Warning:     "PERFORMANCE: DoWork took 1450ms (SLOW)"
```

The 1000 ms threshold is fixed and not configurable. Each branch is gated by
`IsEnabled(LogLevel)` for the level it actually logs at: the slow-operation branch checks
`IsEnabled(LogLevel.Warning)`, the normal branch checks `IsEnabled(LogLevel.Information)`. A
category configured to allow `Warning` while suppressing `Information` still receives
slow-operation warnings.

## Timed scopes (`BeginTimedScope`)

`PerformanceLoggerExtensions.BeginTimedScope` combines entry logging and `FastPerformanceLog` into
a single `using`-scoped call:

```csharp
using (logger.BeginTimedScope(nameof(ImportWordsAsync)))
{
    await ImportWordsAsync();
}
// on enter:  Debug:  "ENTER ImportWordsAsync(ImportWordsAsync)"
// on leave:  Information/Warning: "PERFORMANCE: ImportWordsAsync took Nms [(SLOW)]"
```

`BeginTimedScope` captures the real caller's method name via `[CallerMemberName]` on itself and
forwards it explicitly into the entry log, so the entry line correctly names the method that opened
the scope rather than `TimedScope`'s own constructor.

## `LoggerExtensionException`

A plain three-constructor `Exception` subtype (parameterless, message, message + inner exception).
It is **not thrown anywhere inside this library** — none of the `Fast*` methods or
`BeginTimedScope` fail on a null or misbehaving logger, they simply no-op or pass through. It is
exported for application code that wants a dedicated, catchable exception type for its own
logging-related error paths (e.g. failures while configuring a custom logging provider).

## Known limitations & gotchas

- **`FastMethodEntry`/`FastMethodExit` must be called directly from the method you want to trace**,
  not from a shared helper — `[CallerMemberName]` names whatever method contains the call. If you
  need an indirect call to report the original caller's name (as `BeginTimedScope` does), capture
  `[CallerMemberName]` on your own wrapper and pass it through explicitly as `memberName`.
- **Format strings use `string.Format` placeholders (`{0}`, `{1}`), not structured-logging property
  names.** Passing an `ILogger`-style `"{PropertyName}"` template to `FastInfoMessage(format, args)`
  will not populate structured log fields the way `ILogger.LogInformation` does.
- **`LoggerExtensionException` is unused by the library itself** — don't expect it from any `Fast*`
  call; it is only a convenience type for consumers.
- **`BeginTimedScope` starts its `Stopwatch` unconditionally.** Even if `Debug` logging is disabled,
  a `TimedScope` object and a `Stopwatch` are still created for every call — the cost is small (the
  entry log itself is skipped via the `IsEnabled` check inside `FastMethodEntry`), but there is no
  way to skip the scope object itself when tracing is off.

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| `FastMethodEntry`/`FastMethodExit` logs the wrong method name | The call is happening inside a shared helper rather than the method you intended to trace; pass `memberName` explicitly if needed. |
| `{PropertyName}`-style placeholders show up literally in the log output | `FastInfoMessage`/`FastErrorMessage`/etc. `(format, args)` overloads use `string.Format` syntax (`{0}`, `{1}`), not `ILogger` structured-logging syntax. |
| `BeginTimedScope`'s entry log names the wrong method | Only possible if you call `FastMethodEntry`/`FastMethodExit` directly from your own wrapper without forwarding `[CallerMemberName]` — `BeginTimedScope` itself already forwards the real caller's name. |
