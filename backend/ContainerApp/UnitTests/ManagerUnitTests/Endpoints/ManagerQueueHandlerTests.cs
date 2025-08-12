using Manager.Endpoints;
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

    [Fact(DisplayName = "HandleAsync routes known action and logs unknown")]
    public async Task HandleAsync_Routes_Or_Logs()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Loose);
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();
        var handler = new ManagerQueueHandler(ai.Object, logger);

        var okMsg = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = JsonDocument.Parse("""{"Id":"x","ThreadId":"00000000000000000000000000000000","Answer":"ok"}""").RootElement.Clone()
        };
        await handler.HandleAsync(okMsg, () => Task.CompletedTask, CancellationToken.None);

        var unknownMsg = new Message
        {
            ActionName = (MessageAction)999,
            Payload = JsonDocument.Parse("""{}""").RootElement.Clone()
        };
        await handler.HandleAsync(unknownMsg, () => Task.CompletedTask, CancellationToken.None);
    }

    [Fact(DisplayName = "HandleAnswerAiAsync rethrows on failure for retry policy")]
    public async Task HandleAnswerAi_Rethrows_On_Error()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();

        // Valid payload so deserialization passes
        var goodPayloadJson = JsonSerializer.Serialize(new AiResponseModel
        {
            Id = "q-err",
            ThreadId = Guid.NewGuid().ToString("N"),
            Answer = "any"
        });
        using var goodDoc = JsonDocument.Parse(goodPayloadJson);
        var message = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = goodDoc.RootElement.Clone()
        };

        // Force the failure at the SAVE stage
        ai.Setup(s => s.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()))
          .ThrowsAsync(new InvalidOperationException("boom"));

        var handler = new ManagerQueueHandler(ai.Object, logger);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAnswerAiAsync(message, () => Task.CompletedTask, CancellationToken.None));
    }
}
