using System.Runtime.CompilerServices;

// Grants the test project access to internal types (BrowserConsoleLogQueue, PendingLogEntry, the
// internal BrowserConsoleLogger constructor) so they can be unit-tested directly instead of only
// through the public ILoggerProvider surface.
[assembly: InternalsVisibleTo("MukhaLab.BrowserConsoleLogger.Tests")]
