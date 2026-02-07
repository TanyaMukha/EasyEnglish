namespace MukhaLab.LoggerExtensionDelegate;

public class LoggerExtensionException : Exception
{
    public LoggerExtensionException()
    {
    }

    public LoggerExtensionException(string message)
        : base(message)
    {
    }

    public LoggerExtensionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
