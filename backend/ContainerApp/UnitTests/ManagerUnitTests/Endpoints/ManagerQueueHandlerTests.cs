using System.Text.Json;
using Manager.Endpoints;
using DotQueue;
using Manager.Models;
using Manager.Models.QueueMessages;
using Manager.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Manager.Routing;

namespace ManagerUnitTests.Endpoints;

public class ManagerQueueHandlerTests
{
    // Centralize common setup to remove boilerplate in each test
    private static (
    Mock<IAiGatewayService> ai,
    ManagerQueueHandler handler
    ) CreateSut()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();
        var managerService = Mock.Of<IManagerService>();
        var callbackDispatcher = new Mock<ICallbackDispatcher>();
        var routingAccessor = new Mock<IRoutingContextAccessor>();
        var routingLogger = new Mock<ILogger<RoutingMiddleware>>();
        var routing = new RoutingMiddleware(routingAccessor.Object, routingLogger.Object);

        var handler = new ManagerQueueHandler(ai.Object, logger, managerService, callbackDispatcher.Object, routing);
        return (ai, handler);
    }

    private static JsonElement ToJsonElement<T>(T value) =>
        JsonSerializer.SerializeToElement(value);

    [Fact(DisplayName = "HandleAnswerAiAsync => saves valid AI answer")]
    public async Task HandleAnswerAi_Saves_When_Valid()
    {
        // Arrange
        var (ai, handler) = CreateSut();

        var response = new AiResponseModel
        {
            Id = "q-1",
            ThreadId = Guid.NewGuid().ToString("N"),
            Answer = "hey"
        };

        var message = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = ToJsonElement(response)
        };

        ai.Setup(s => s.SaveAnswerAsync(
                It.Is<AiResponseModel>(m =>
                    m.Id == response.Id &&
                    m.ThreadId == response.ThreadId &&
                    m.Answer == response.Answer),
                It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask)
          .Verifiable();

        // Act
        await handler.HandleAnswerAiAsync(message, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        ai.Verify();
        ai.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "HandleAnswerAiAsync wraps failure in RetryableException for retry policy")]
    public async Task HandleAnswerAi_Wraps_On_Error()
    {
        var (ai, handler) = CreateSut();

        var payload = new AiResponseModel
        {
            Id = "q-err",
            ThreadId = Guid.NewGuid().ToString("N"),
            Answer = "any"
        };

        var message = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = ToJsonElement(payload)
        };

        ai.Setup(s => s.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()))
          .ThrowsAsync(new InvalidOperationException("boom"));

        var ex = await Assert.ThrowsAsync<RetryableException>(() =>
            handler.HandleAnswerAiAsync(message, () => Task.CompletedTask, CancellationToken.None));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("boom", ex.InnerException!.Message);
        Assert.Contains("Transient error while saving answer", ex.Message);

        ai.Verify(s => s.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()), Times.Once);
        ai.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "HandleAsync routes known action; unknown action throws NonRetryable")]
    public async Task HandleAsync_Routes_Known_And_Throws_On_Unknown()
    {
        var (ai, handler) = CreateSut();

        // known action: valid payload so it routes and calls SaveAnswerAsync once
        var ok = new AiResponseModel
        {
            Id = "x",
            ThreadId = Guid.NewGuid().ToString("N"),
            Answer = "ok"
        };

        var okMsg = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = ToJsonElement(ok)
        };

        ai.Setup(a => a.SaveAnswerAsync(
                It.Is<AiResponseModel>(m => m.Id == "x" && m.Answer == "ok"),
                It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask)
          .Verifiable();

        await handler.HandleAsync(okMsg, null, () => Task.CompletedTask, CancellationToken.None);

        ai.Verify(a => a.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()), Times.Once);
        ai.VerifyNoOtherCalls();

        // unknown action: handler should throw NonRetryableException
        var unknownMsg = new Message
        {
            ActionName = (MessageAction)999,
            Payload = JsonDocument.Parse("""{}""").RootElement.Clone()
        };

        await Assert.ThrowsAsync<NonRetryableException>(() =>
            handler.HandleAsync(unknownMsg, null, () => Task.CompletedTask, CancellationToken.None));
    }
}