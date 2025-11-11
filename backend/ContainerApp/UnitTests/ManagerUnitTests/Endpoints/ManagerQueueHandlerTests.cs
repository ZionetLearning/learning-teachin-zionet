using System.Text.Json;
using Manager.Endpoints;
using Manager.Models.Chat;
using Manager.Models.Games;
using Manager.Models.Notifications;
using Manager.Models.QueueMessages;
using Manager.Models.Sentences;
using Manager.Services;
using Manager.Services.Clients.Accessor;
using Manager.Services.Clients.Accessor.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace ManagerUnitTests.Endpoints;

public class ManagerQueueHandlerTests
{
    private static ManagerQueueHandler CreateSut(
        Mock<INotificationService>? notificationMock = null,
        Mock<IGameAccessorClient>? gameAccessorClientMock = null)
    {
        var logger = Mock.Of<ILogger<ManagerQueueHandler>>();
        var notificationService = notificationMock?.Object ?? Mock.Of<INotificationService>();
        var gameAccessorClient = gameAccessorClientMock?.Object ?? Mock.Of<IGameAccessorClient>();

        return new ManagerQueueHandler(logger, notificationService, gameAccessorClient);
    }

    private static JsonElement ToJsonElement<T>(T value) =>
        JsonSerializer.SerializeToElement(value);

    [Fact(DisplayName = "HandleNotifyUserAsync => sends notification via service")]
    public async Task HandleNotifyUserAsync_Sends_Notification()
    {
        var mockNotif = new Mock<INotificationService>();
        var handler = CreateSut(mockNotif);

        var notification = new UserNotification { Message = "hello" };
        var userId = Guid.NewGuid().ToString();
        var metadata = new UserContextMetadata { MessageId = "m1", UserId = userId };

        var message = new Message
        {
            ActionName = MessageAction.NotifyUser,
            Payload = ToJsonElement(notification),
            Metadata = JsonSerializer.SerializeToElement(metadata)
        };

        await handler.HandleAsync(message, null, () => Task.CompletedTask, CancellationToken.None);

        mockNotif.Verify(
            s => s.SendNotificationAsync(metadata.UserId, It.IsAny<UserNotification>()),
            Times.Once);
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

        var userId = Guid.NewGuid().ToString();
        var metadata = new UserContextMetadata { UserId = userId };

        var message = new Message
        {
            ActionName = MessageAction.ProcessingChatMessage,
            Payload = ToJsonElement(chatResponse),
            Metadata = JsonSerializer.SerializeToElement(metadata)
        };

        await handler.HandleAsync(message, null, () => Task.CompletedTask, CancellationToken.None);

        mockNotif.Verify(
            s => s.SendEventAsync(EventType.ChatAiAnswer, userId, chatResponse),
            Times.Once);
    }

    [Fact(DisplayName = "HandleGenerateAnswer => saves to accessor and sends sentence event")]
    public async Task HandleGenerateAnswer_Saves_And_Sends_Event()
    {
        var mockNotif = new Mock<INotificationService>();
        var mockAccessor = new Mock<IGameAccessorClient>();
        var handler = CreateSut(mockNotif, mockAccessor);

        var userId = Guid.NewGuid().ToString();
        var requestId = Guid.NewGuid().ToString();
        var sentenceResponse = new SentenceResponse
        {
            RequestId = requestId,
            Sentences = new List<SentenceItem>
            {
                new()
                {
                    GameType = "WordOrderGame",
                    Text = "generated sentence",
                    Difficulty = "easy",
                    Nikud = true
                }
            }
        };

        var expectedResult = new List<AttemptedSentenceResult>
        {
            new()
            {
                ExerciseId = Guid.NewGuid(),
                Text = "generated sentence",
                Words = new List<string> { "generated sentence" },
                Difficulty = "easy",
                Nikud = true
            }
        };

        // Setup mock to return expected result when SaveGeneratedSentencesAsync is called
        mockAccessor
            .Setup(x => x.SaveGeneratedSentencesAsync(It.IsAny<GeneratedSentenceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var message = new Message
        {
            ActionName = MessageAction.GenerateSentences,
            Payload = ToJsonElement(sentenceResponse),
            Metadata = JsonSerializer.SerializeToElement(userId)
        };

        await handler.HandleAsync(message, null, () => Task.CompletedTask, CancellationToken.None);

        // Verify SaveGeneratedSentencesAsync was called with correct data
        mockAccessor.Verify(
            x => x.SaveGeneratedSentencesAsync(
                It.Is<GeneratedSentenceDto>(dto =>
                    dto.StudentId == Guid.Parse(userId) &&
                    dto.GameType == "WordOrderGame" &&
                    dto.Sentences.Count == 1 &&
                    dto.Sentences[0].Text == "generated sentence" &&
                    dto.Sentences[0].CorrectAnswer.Count == 1 &&
                    dto.Sentences[0].CorrectAnswer[0] == "generated sentence"
                ),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        // Verify event was sent with SentenceGenerationResponse containing RequestId
        mockNotif.Verify(
            s => s.SendEventAsync(
                EventType.SentenceGeneration,
                userId,
                It.Is<SentenceGenerationResponse>(response =>
                    response.RequestId == requestId &&
                    response.Sentences.Count == 1 &&
                    response.Sentences[0].Text == "generated sentence")),
            Times.Once);
    }
}