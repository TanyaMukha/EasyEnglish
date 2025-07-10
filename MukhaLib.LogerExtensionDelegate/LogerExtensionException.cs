namespace LogerExtensionDelegate;

public class LogerExtensionException : Exception
{
    public LogerExtensionException()
    {
    }

    public LogerExtensionException(string message)
        : base(message)
    {
    }

    public LogerExtensionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
