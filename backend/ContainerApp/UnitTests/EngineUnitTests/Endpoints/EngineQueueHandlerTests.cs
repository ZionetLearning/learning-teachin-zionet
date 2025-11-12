using Dapr.Client;
using DotQueue;
using Engine.Constants;
using Engine.Endpoints;
using Engine.Helpers;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Models.Sentences;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using FluentAssertions;
using Google.Rpc;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Moq;
using System.Text.Json;

public class EngineQueueHandlerTests
{
    private static JsonElement ToJsonElement<T>(T obj) =>
        JsonSerializer.SerializeToElement(obj);

    private static JsonElement EmptyHistory()
    {
        using var doc = JsonDocument.Parse("""{"messages":[]}""");
        return doc.RootElement.Clone();
    }

    private static JsonElement Je(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        return doc.RootElement.Clone();
    }

    // Centralized SUT + mocks factory to reduce boilerplate
    private static (
        Mock<DaprClient> dapr,
        Mock<IChatAiService> ai,
        Mock<IAiReplyPublisher> pub,
        Mock<IAccessorClient> accessorClient,
        Mock<ISentencesService> sentService,
        Mock<IChatTitleService> titleService,
        Mock<IWordExplainService> explainService,
        Mock<ILogger<EngineQueueHandler>> log,
        Mock<ILogger<StreamingChatAIBatcher>> batcherLog,
        EngineQueueHandler sut
    ) CreateSut()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Loose);
        var ai = new Mock<IChatAiService>(MockBehavior.Strict);
        var pub = new Mock<IAiReplyPublisher>(MockBehavior.Strict);
        var accessorClient = new Mock<IAccessorClient>(MockBehavior.Strict);
        var sentService = new Mock<ISentencesService>(MockBehavior.Strict);
        var titleService = new Mock<IChatTitleService>(MockBehavior.Strict);
        var log = new Mock<ILogger<EngineQueueHandler>>();
        var batcherLog = new Mock<ILogger<StreamingChatAIBatcher>>();
        var explainService = new Mock<IWordExplainService>(MockBehavior.Strict);

        var sut = new EngineQueueHandler(
            dapr.Object,
            log.Object,
            batcherLog.Object,
            ai.Object,
            pub.Object,
            accessorClient.Object,
            sentService.Object,
            titleService.Object,
            explainService.Object
        );
        return (dapr, ai, pub, accessorClient, sentService, titleService, explainService, log, batcherLog, sut);
    }

    [Fact]
    public async Task HandleAsync_CreateTask_Processes_TaskModel()
    {
        // Arrange
        var (daprClient, ai, pub, accessorClient, sentService, titleService, explainService, log, batcherLog, sut) = CreateSut();

        var task = new TaskModel
        {
            Id = 42,
            Name = "demo",
            Payload = "{}"
        };

        var msg = new Message
        {
            ActionName = MessageAction.CreateTask,
            Payload = ToJsonElement(task)
        };

        // Act
        var act = async () => await sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
        accessorClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_CreateTask_InvalidPayload_DoesNotCall_Dapr()
    {
        var (daprClient, ai, pub, accessorClient, sentService, titleService, explainService, log, batcherLog, sut) = CreateSut();

        // payload = "null"
        var msg = new Message
        {
            ActionName = MessageAction.CreateTask,
            Payload = JsonSerializer.Deserialize<JsonElement>("null")
        };

        var act = () => sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
            .WithMessage("*Payload deserialization returned null*");

        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
        accessorClient.VerifyNoOtherCalls();
    }

    [Fact(Skip = "Todo: do after refactoring ai Chat for queue")]
    public async Task HandleAsync_ProcessingQuestionAi_HappyPath_Calls_Ai_And_Publishes()
    {
        // Arrange
        var (daprClient, ai, pub, accessorClient, sentService, titleService, explainService, log, batcherLog, sut) = CreateSut();

        var requestId = Guid.NewGuid().ToString();
        var threadId = Guid.NewGuid();
        var chatName = "chat name";
        var userId = Guid.NewGuid();
        var userMsg = "hello";
        var textAnswer = "world";

        var engineReq = new EngineChatRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserId = userId,
            UserMessage = userMsg,
            ChatType = ChatType.Default,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TtlSeconds = 120,
        };

        var snapshotFromAccessor = new HistorySnapshotDto
        {
            ThreadId = threadId,
            UserId = userId,
            ChatType = "default",
            History = EmptyHistory()
        };

        accessorClient.Setup(a => a.GetHistorySnapshotAsync(threadId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(snapshotFromAccessor);

        var history = new ChatHistory();
        history.AddSystemMessage("You are a helpful assistant.");
        history.AddUserMessageNow(userMsg);
        var expectedAiReq = new ChatAiServiceRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = engineReq.SentAt,
            TtlSeconds = 120,
            History = history
        };


        var updatedHistory = history;

        updatedHistory.AddAssistantMessage("World");

        var answer = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            UserId = userId,
            Role = MessageRole.Assistant,
            Content = textAnswer
        };

        history.AddAssistantMessage(textAnswer);

        var aiResp = new ChatAiServiceResponse
        {
            RequestId = requestId,
            ThreadId = threadId,
            Status = ChatAnswerStatus.Ok,
            Answer = answer,
            UpdatedHistory = updatedHistory
        };

        ai.Setup(a => a.ChatHandlerAsync(It.Is<ChatAiServiceRequest>(r =>
                        r.RequestId == expectedAiReq.RequestId &&
                        r.ThreadId == expectedAiReq.ThreadId &&
                        r.UserId == expectedAiReq.UserId &&
                        r.ChatType == expectedAiReq.ChatType &&
                        r.History[0].Content == expectedAiReq.History[0].Content
                        &&
                        r.History[1].Content == expectedAiReq.History[1].Content
                        &&
                        r.History[2].Content == expectedAiReq.History[2].Content
                    ), It.IsAny<CancellationToken>()))
          .ReturnsAsync(aiResp);

        var engineResponse = new EngineChatResponse
        {
            RequestId = requestId,
            AssistantMessage = textAnswer,
            ChatName = chatName,
            ThreadId = threadId,
            Status = ChatAnswerStatus.Ok
        };
        var chatMetadata = new UserContextMetadata
        {
            UserId = userId.ToString()
        };

        pub.Setup(p => p.SendReplyAsync(chatMetadata, engineResponse, It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingChatMessage,
            Payload = ToJsonElement(engineReq)
        };

        // Act
        await sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        accessorClient.Verify(a => a.GetHistorySnapshotAsync(threadId, userId, It.IsAny<CancellationToken>()), Times.Once);
        ai.VerifyAll();
        accessorClient.Verify(a => a.UpsertHistorySnapshotAsync(It.IsAny<UpsertHistoryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        pub.Verify(p => p.SendReplyAsync(chatMetadata, engineResponse, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(Skip = "Todo: do after refactoring ai Chat for queue")]
    public async Task HandleAsync_ProcessingQuestionAi_MissingThreadId_ThrowsRetryable()
    {
        var (daprClient, ai, pub, accessorClient, sentService, titleService, explainService, log, batcherLog, sut) = CreateSut();

        var requestId = Guid.NewGuid().ToString();
        var userId = Guid.NewGuid();

        var req = new EngineChatRequest
        {
            RequestId = requestId,
            ThreadId = Guid.Empty,
            UserId = userId,
            UserMessage = "hello",
            ChatType = ChatType.Default,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TtlSeconds = 120,
        };

        accessorClient.Setup(ac => ac.GetChatHistoryAsync(Guid.Empty, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("Invalid ThreadId"));

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingChatMessage,
            Payload = ToJsonElement(req)
        };

        var act = () => sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<RetryableException>()
          .WithMessage("*Transient error while processing AI question*");

        accessorClient.Verify(ac => ac.GetChatHistoryAsync(Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);

        accessorClient.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    [Fact(Skip = "Todo: need fix after merge chatAI streaming")]
    public async Task HandleAsync_ProcessingQuestionAi_AiThrows_WrappedInRetryable()
    {
        // Arrange
        var (daprClient, ai, pub, accessorClient, sentService, titleService, explainService, log, batcherLog, sut) = CreateSut();

        var requestId = Guid.NewGuid().ToString();
        var threadId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var history = new ChatHistory();
        history.AddSystemMessage("You are a helpful assistant.");
        history.AddUserMessageNow("boom");
        
        var engineReq = new EngineChatRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserId = userId,
            UserMessage = "boom",
            ChatType = ChatType.Default,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TtlSeconds = 120,
        };

        var snapshotFromAccessor = new HistorySnapshotDto
        {
            ThreadId = threadId,
            UserId = userId,
            ChatType = "default",
            History = EmptyHistory()
        };

        accessorClient.Setup(a => a.GetHistorySnapshotAsync(threadId, userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(snapshotFromAccessor);

        accessorClient
            .Setup(a => a.GetUserInterestsAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string>());


        accessorClient
            .Setup(a => a.UpsertHistorySnapshotAsync(
                It.IsAny<UpsertHistoryRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HistorySnapshotDto
            {
                ThreadId = threadId,
                UserId = userId,
                ChatType = "default",
                History = EmptyHistory()
            });

        var expectedAiReq = new ChatAiServiceRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = engineReq.SentAt,
            TtlSeconds = 120,
            History = history
        };

        ai.Setup(a => a.ChatHandlerAsync(It.Is<ChatAiServiceRequest>(r =>
                        r.ThreadId == threadId &&
                        r.History[1].Content == "boom"
                    ), It.IsAny<CancellationToken>()))
          .ThrowsAsync(new InvalidOperationException("AI failed"));

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingChatMessage,
            Payload = ToJsonElement(engineReq),
            Metadata = JsonSerializer.SerializeToElement(new { UserId = userId })
        };

        var act = () => sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);
                var ex = (await act.Should().ThrowAsync<RetryableException>()
                           .WithMessage("*Transient error while processing AI chat*"))
                 .Which;

        ex.InnerException.Should().BeOfType<InvalidOperationException>();
        ex.InnerException!.Message.Should().Contain("AI failed");

        accessorClient.Verify(a => a.GetHistorySnapshotAsync(threadId, userId, It.IsAny<CancellationToken>()), Times.Once);
        pub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_UnknownAction_ThrowsNonRetryable_AndSkipsWork()
    {
        var (daprClient, ai, pub, accessorClient, sentService, titleService, explainService, log, batcherLog, sut) = CreateSut();


        var msg = new Message
        {
            ActionName = (MessageAction)9999,
            Payload = JsonSerializer.Deserialize<JsonElement>("{}")
        };

        var act = () => sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                         .WithMessage("*No handler registered for action*");

        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
        accessorClient.VerifyNoOtherCalls();
    }
}