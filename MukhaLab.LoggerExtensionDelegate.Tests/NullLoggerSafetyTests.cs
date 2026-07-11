using Microsoft.Extensions.Logging;

namespace MukhaLab.LoggerExtensionDelegate.Tests;

public class NullLoggerSafetyTests
{
    [Fact]
    public void AllFastMethods_OnNullLogger_DoNotThrow()
    {
        ILogger? logger = null;

        var exception = Record.Exception(() =>
        {
            logger.FastInfoMessage("hello");
            logger.FastInfoMessage("hello {0}", 1);
            logger.FastErrorMessage("hello");
            logger.FastErrorMessage("hello {0}", 1);
            logger.FastErrorMessage(new Exception(), "hello {0}", 1);
            logger.FastWarningMessage("hello");
            logger.FastWarningMessage("hello {0}", 1);
            logger.FastWarningMessage(new Exception(), "hello {0}", 1);
            logger.FastDebugMessage("hello");
            logger.FastDebugMessage("hello {0}", 1);
            logger.FastDebugMessage(new Exception(), "hello {0}", 1);
            logger.FastInfoWithContext("hello");
            logger.FastInfoWithUserContext("user", "hello");
            logger.FastPerformanceLog("op", 100);
            logger.FastMethodEntry();
            logger.FastMethodExit();
        });

        Assert.Null(exception);
    }
}
