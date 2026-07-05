namespace EasyEnglish.App.Diagnostics;

/// <summary>
/// Best-effort запис необроблених винятків у файл на диску.
/// Не покладається на DI/ILogger, бо викликається з глобальних обробників
/// крашів, де стан застосунку (в т.ч. контейнер служб) може бути непридатним.
/// </summary>
public static class CrashLogger
{
    private static readonly object _lock = new();

    public static void Log(string source, Exception? exception)
    {
        try
        {
            var path = GetLogFilePath();
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}{Environment.NewLine}{exception}{Environment.NewLine}{new string('-', 80)}{Environment.NewLine}";

            lock (_lock)
            {
                File.AppendAllText(path, entry);
            }
        }
        catch
        {
            // Логування не повинно саме спричиняти новий збій.
        }
    }

    private static string GetLogFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(appDataPath, "EasyEnglish", "logs", "crash.log");
    }
}
