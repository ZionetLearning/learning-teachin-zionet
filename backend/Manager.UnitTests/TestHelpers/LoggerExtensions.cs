using Microsoft.Extensions.Logging;
using Moq;

namespace Manager.UnitTests.TestHelpers;

public static class LoggerExtensions
{
    public static void VerifyLog(
        this Mock<ILogger> logger,
        LogLevel level,
        Times times)
    {
        logger.Verify(x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            times);
    }

    public static Mock<ILogger<T>> CreateLoggerMock<T>() =>
        new(MockBehavior.Loose) { DefaultValue = DefaultValue.Mock };
}
