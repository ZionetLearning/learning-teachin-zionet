using System.Text.Json;
using DotQueue;
using Manager.Common;
using Manager.Endpoints;
using Manager.Models;
using Manager.Services;
using Manager.Models.QueueMessages;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManagerUnitTests.Endpoints;

public class ManagerQueueHandlerTests
{
    private static ManagerQueueHandler CreateSut()
    {
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();
        var notificationService = Mock.Of<INotificationService>();
        return new ManagerQueueHandler(logger, notificationService);
    }

    private static JsonElement ToJsonElement<T>(T value) =>
        JsonSerializer.SerializeToElement(value);

    [Fact(DisplayName = "HandleAnswerAiAsync => saves valid AI answer")]
    public async Task HandleAnswerAi_Saves_When_Valid()
    {
        // Arrange
        var handler = CreateSut();
        AiAnswerStore.Answers.Clear();

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

        // Act
        await handler.HandleAnswerAiAsync(message, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        Assert.True(AiAnswerStore.Answers.ContainsKey("q-1"));
        Assert.Equal("hey", AiAnswerStore.Answers["q-1"]);
    }

    [Fact(DisplayName = "HandleAnswerAiAsync wraps failure in NonRetryableException for bad JSON")]
    public async Task HandleAnswerAi_Wraps_On_BadJson()
    {
        var handler = CreateSut();

        // Invalid payload → missing ThreadId
        var badJson = JsonDocument.Parse("""{"Id":"q-err"}""").RootElement;

        var message = new Message
        {
            ActionName = MessageAction.AnswerAi,
            Payload = badJson
        };

        var ex = await Assert.ThrowsAsync<NonRetryableException>(() =>
            handler.HandleAnswerAiAsync(message, () => Task.CompletedTask, CancellationToken.None));

        Assert.Contains("Invalid JSON payload", ex.Message);
    }

    [Fact(DisplayName = "HandleAsync routes known action; unknown action throws NonRetryable")]
    public async Task HandleAsync_Routes_Known_And_Throws_On_Unknown()
    {
        var handler = CreateSut();
        AiAnswerStore.Answers.Clear();

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

        await handler.HandleAsync(okMsg, () => Task.CompletedTask, CancellationToken.None);

        Assert.True(AiAnswerStore.Answers.ContainsKey("x"));
        Assert.Equal("ok", AiAnswerStore.Answers["x"]);

        var unknownMsg = new Message
        {
            ActionName = (MessageAction)999,
            Payload = JsonDocument.Parse("""{}""").RootElement.Clone()
        };

        await Assert.ThrowsAsync<NonRetryableException>(() =>
            handler.HandleAsync(unknownMsg, () => Task.CompletedTask, CancellationToken.None));
    }
}