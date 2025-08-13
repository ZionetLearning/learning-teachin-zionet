using Manager.Endpoints;
using Manager.Messaging;
using Manager.Models;
using Manager.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace ManagerUnitTests.Endpoints;
public class ManagerQueueHandlerTests
{
    [Fact(DisplayName = "HandleAnswerAiAsync => saves valid AI answer")]
    public async Task HandleAnswerAi_Saves_When_Valid()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();

        var response = new AiResponseModel
        {
            Id = "q-1",
            ThreadId = Guid.NewGuid().ToString("N"),
            Answer = "hey"
        };

        var json = JsonSerializer.Serialize(response);
        using var doc = JsonDocument.Parse(json);
        var payload = doc.RootElement.Clone();

        var message = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = payload
        };

        ai.Setup(s => s.SaveAnswerAsync(
                It.Is<AiResponseModel>(m => m.Id == response.Id && m.Answer == response.Answer),
                It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask)
          .Verifiable();

        var handler = new ManagerQueueHandler(ai.Object, logger);

        await handler.HandleAnswerAiAsync(message, () => Task.CompletedTask, CancellationToken.None);

        ai.Verify();
    }

    [Fact(DisplayName = "HandleAnswerAiAsync wraps failure in RetryableException for retry policy")]
    public async Task HandleAnswerAi_Wraps_On_Error()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();

        var payload = new AiResponseModel
        {
            Id = "q-err",
            ThreadId = Guid.NewGuid().ToString("N"),
            Answer = "any"
        };
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(payload));

        var message = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = doc.RootElement.Clone()
        };

        ai.Setup(s => s.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()))
          .ThrowsAsync(new InvalidOperationException("boom"));

        var handler = new ManagerQueueHandler(ai.Object, logger);

        var ex = await Assert.ThrowsAsync<RetryableException>(() =>
            handler.HandleAnswerAiAsync(message, () => Task.CompletedTask, CancellationToken.None));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("boom", ex.InnerException!.Message);
        Assert.Contains("Transient error while saving answer", ex.Message);

        ai.Verify(s => s.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "HandleAsync routes known action; unknown action throws NonRetryable")]
    public async Task HandleAsync_Routes_Known_And_Throws_On_Unknown()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();
        var handler = new ManagerQueueHandler(ai.Object, logger);

        // known action: valid payload so it routes and calls SaveAnswerAsync once
        var ok = new AiResponseModel
        {
            Id = "x",
            ThreadId = Guid.NewGuid().ToString("N"),
            Answer = "ok"
        };
        using var okDoc = JsonDocument.Parse(JsonSerializer.Serialize(ok));
        var okMsg = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = okDoc.RootElement.Clone()
        };

        ai.Setup(a => a.SaveAnswerAsync(
                It.Is<AiResponseModel>(m => m.Id == "x" && m.Answer == "ok"),
                It.IsAny<CancellationToken>()))
          .Returns(Task.CompletedTask)
          .Verifiable();

        await handler.HandleAsync(okMsg, () => Task.CompletedTask, CancellationToken.None);

        ai.Verify(a => a.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()), Times.Once);

        // unknown action: handler should throw NonRetryableException
        var unknownMsg = new Message
        {
            ActionName = (MessageAction)999,
            Payload = JsonDocument.Parse("""{}""").RootElement.Clone()
        };

        await Assert.ThrowsAsync<NonRetryableException>(() =>
            handler.HandleAsync(unknownMsg, () => Task.CompletedTask, CancellationToken.None));
    }
}
