
﻿using System.Text.Json;
using Manager.Endpoints;
using DotQueue;
using Manager.Models;
using Manager.Models.QueueMessages;
using Manager.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManagerUnitTests.Endpoints;

public class ManagerQueueHandlerTests
{
    // Centralize common setup
    private static (
        Mock<IAiGatewayService> ai,
        Mock<ICallbackDispatcher> dispatcher,
        ManagerQueueHandler handler
    ) CreateSut()
    {
        var ai = new Mock<IAiGatewayService>(MockBehavior.Strict);
        var dispatcher = new Mock<ICallbackDispatcher>(MockBehavior.Loose);
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();
        var managerService = Mock.Of<IManagerService>();
        var handler = new ManagerQueueHandler(ai.Object, logger, managerService, dispatcher.Object);

        return (ai, dispatcher, handler);
    }

    private static JsonElement ToJsonElement<T>(T value) =>
        JsonSerializer.SerializeToElement(value);

    [Fact(DisplayName = "HandleAnswerAiAsync => saves valid AI answer")]
    public async Task HandleAnswerAi_Saves_When_Valid()
    {
        var (ai, _, handler) = CreateSut();

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

        await handler.HandleAnswerAiAsync(message, null, () => Task.CompletedTask, CancellationToken.None);

        ai.Verify();
        ai.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "HandleAnswerAiAsync wraps failure in RetryableException for retry policy")]
    public async Task HandleAnswerAi_Wraps_On_Error()
    {
        var (ai, _, handler) = CreateSut();

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
            handler.HandleAnswerAiAsync(message, null, () => Task.CompletedTask, CancellationToken.None));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("boom", ex.InnerException!.Message);
        Assert.Contains("Transient error while saving answer", ex.Message);

        ai.Verify(s => s.SaveAnswerAsync(It.IsAny<AiResponseModel>(), It.IsAny<CancellationToken>()), Times.Once);
        ai.VerifyNoOtherCalls();
    }

    [Fact(DisplayName = "HandleAsync routes known action; unknown action throws NonRetryable")]
    public async Task HandleAsync_Routes_Known_And_Throws_On_Unknown()
    {
        var (ai, _, handler) = CreateSut();

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

        var unknownMsg = new Message
        {
            ActionName = (MessageAction)999,
            Payload = JsonDocument.Parse("""{}""").RootElement.Clone()
        };

        await Assert.ThrowsAsync<NonRetryableException>(() =>
            handler.HandleAsync(unknownMsg, null, () => Task.CompletedTask, CancellationToken.None));
    }
}