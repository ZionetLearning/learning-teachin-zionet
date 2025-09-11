using System.Text.Json;
using DotQueue;
using Manager.Endpoints;
using Manager.Models.Chat;
using Manager.Models.Notifications;
using Manager.Models.QueueMessages;
using Manager.Models.Sentences;
using Manager.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManagerUnitTests.Endpoints;

public class ManagerQueueHandlerTests
{
    private static ManagerQueueHandler CreateSut(Mock<INotificationService>? notificationMock = null)
    {
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();
        var notificationService = notificationMock?.Object ?? Mock.Of<INotificationService>();
        return new ManagerQueueHandler(logger, notificationService);
    }

    private static JsonElement ToJsonElement<T>(T value) =>
        JsonSerializer.SerializeToElement(value);

    [Fact(DisplayName = "HandleNotifyUserAsync => sends notification via service")]
    public async Task HandleNotifyUserAsync_Sends_Notification()
    {
        var mockNotif = new Mock<INotificationService>();
        var handler = CreateSut(mockNotif);

        var notification = new UserNotification { Message = "hello" };
        var userId = Guid.NewGuid().ToString(); // string
        var metadata = new UserContextMetadata { MessageId = "m1", UserId = userId };

        var message = new Message
        {
            ActionName = MessageAction.NotifyUser,
            Payload = ToJsonElement(notification),
            Metadata = JsonSerializer.SerializeToElement(metadata)
        };

        await handler.HandleAsync(message, null, () => Task.CompletedTask, CancellationToken.None);
        
        mockNotif.Verify(s => s.SendNotificationAsync(metadata.UserId, It.IsAny<UserNotification>()), Times.Once);
    }

    [Fact(DisplayName = "HandleAIChatAnswerAsync => sends chat event via service")]
    public async Task HandleAIChatAnswerAsync_Sends_ChatEvent()
    {
        var mockNotif = new Mock<INotificationService>();
        var handler = CreateSut(mockNotif);

        var chatResponse = new AIChatResponse
        {
            RequestId = "req-1",
            ChatName = "test-chat",
            AssistantMessage = "hi",
            ThreadId = Guid.NewGuid()
        };

        var userId = Guid.NewGuid().ToString(); // string
        var metadata = new UserContextMetadata { UserId = userId };

        var message = new Message
        {
            ActionName = MessageAction.ProcessingChatMessage,
            Payload = ToJsonElement(chatResponse),
            Metadata = JsonSerializer.SerializeToElement(metadata)
        };

        await handler.HandleAsync(message, null, () => Task.CompletedTask, CancellationToken.None);

        mockNotif.Verify(s => s.SendEventAsync(EventType.ChatAiAnswer, userId, chatResponse), Times.Once);
    }

    [Fact(DisplayName = "HandleGenerateAnswer => sends sentence event via service")]
    public async Task HandleGenerateAnswer_Sends_SentenceEvent()
    {
        var mockNotif = new Mock<INotificationService>();
        var handler = CreateSut(mockNotif);

        var sentenceResponse = new SentenceResponse
        {
            Sentences = new List<SentenceItem>
            {
                new() { Text = "generated", Difficulty = "easy", Nikud = true }
            }
        };

        var userId = Guid.NewGuid().ToString(); // string

        var message = new Message
        {
            ActionName = MessageAction.GenerateSentences,
            Payload = ToJsonElement(sentenceResponse),
            Metadata = JsonSerializer.SerializeToElement(userId)
        };

        await handler.HandleAsync(message, null, () => Task.CompletedTask, CancellationToken.None);

        mockNotif.Verify(s => s.SendEventAsync(EventType.SentenceGeneration, userId, It.IsAny<SentenceResponse>()), Times.Once);
    }
}