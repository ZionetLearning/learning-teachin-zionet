using DotQueue;
using Engine.Constants;
using Engine.Endpoints;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

public class EngineQueueHandlerTests
{
    private static JsonElement ToJsonElement<T>(T obj) =>
        JsonSerializer.SerializeToElement(obj);

    // Centralized SUT + mocks factory to reduce boilerplate
    private static (
        Mock<IEngineService> engine,
        Mock<IChatAiService> ai,
        Mock<IAiReplyPublisher> pub,
        Mock<IAccessorClient> accessorClient,
        Mock<ILogger<EngineQueueHandler>> log,
        EngineQueueHandler sut
    ) CreateSut()
    {
        var engine = new Mock<IEngineService>(MockBehavior.Strict);
        var ai = new Mock<IChatAiService>(MockBehavior.Strict);
        var pub = new Mock<IAiReplyPublisher>(MockBehavior.Strict);
        var accessorClient = new Mock<IAccessorClient>(MockBehavior.Strict);
        var log = new Mock<ILogger<EngineQueueHandler>>();
        var sut = new EngineQueueHandler(engine.Object, ai.Object, pub.Object, accessorClient.Object, log.Object);
        return (engine, ai, pub, accessorClient, log, sut);
    }

    [Fact]
    public async Task HandleAsync_CreateTask_Processes_TaskModel()
    {
        // Arrange
        var (engine, ai, pub, accessorClient, log, sut) = CreateSut();

        var task = new TaskModel
        {
            Id = 42,
            Name = "demo",
            Payload = "{}"
        };

        engine.Setup(e => e.ProcessTaskAsync(task, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

        var msg = new Message
        {
            ActionName = MessageAction.CreateTask,
            Payload = ToJsonElement(task)
        };

        // Act
        await sut.HandleAsync(msg, renewLock: () => Task.CompletedTask, CancellationToken.None);

        // Assert
        engine.Verify(e => e.ProcessTaskAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        engine.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_CreateTask_InvalidPayload_DoesNotCall_Engine()
    {
        var (engine, ai, pub, accessorClient, log, sut) = CreateSut();

        // payload = "null"
        var msg = new Message
        {
            ActionName = MessageAction.CreateTask,
            Payload = JsonSerializer.Deserialize<JsonElement>("null")
        };

        var act = () => sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*Payload deserialization returned null*");

        engine.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ProcessingQuestionAi_HappyPath_Calls_Ai_And_Publishes()
    {
        // Arrange
        var (engine, ai, pub, accessorClient, log, sut) = CreateSut();

        var requestId = Guid.NewGuid().ToString();
        var threadId = Guid.NewGuid();
        var userId = "testUserId";
        var textAnserFromAI = "world";

        var chatHistory = new List<ChatMessage>();

        var requestToEngine = new EngineChatRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserMessage = "hello",
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TtlSeconds = 120,
        };

        var reqestToAiService = new ChatAiServiseRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserMessage = "hello",
            History = chatHistory,
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = requestToEngine.SentAt,
            TtlSeconds = 120,
        };

        var answer = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            UserId = userId,
            Role = MessageRole.Assistant,
            Content = textAnserFromAI
        };

        var aiResponse = new ChatAiServiceResponse
        {
            RequestId = requestId,
            ThreadId = threadId,
            Status = ChatAnswerStatus.Ok,
            Answer = answer,
            Error = null
        };

        accessorClient.Setup(ac => ac.GetChatHistoryAsync(threadId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(chatHistory);

        ai.Setup(a => a.ChatHandlerAsync(reqestToAiService, It.IsAny<CancellationToken>()))
          .ReturnsAsync(aiResponse);

        accessorClient.Setup(ac => ac.StoreMessageAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((ChatMessage msg, CancellationToken ct) => msg);

        var engineResponse = new EngineChatResponse
        {
            RequestId = requestId,
            AssistantMessage = textAnserFromAI,
            ThreadId = threadId,
            Status = ChatAnswerStatus.Ok

        };

        pub.Setup(p => p.SendReplyAsync(engineResponse, $"{QueueNames.ManagerCallbackQueue}-out", It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingQuestionAi,
            Payload = ToJsonElement(requestToEngine)
        };

        // Act
        await sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        ai.Verify(a => a.ChatHandlerAsync(reqestToAiService, It.IsAny<CancellationToken>()), Times.Once);
        ai.VerifyNoOtherCalls();
        pub.Verify(p => p.SendReplyAsync(engineResponse, $"{QueueNames.ManagerCallbackQueue}-out", It.IsAny<CancellationToken>()), Times.Once);
        pub.VerifyNoOtherCalls();
        engine.VerifyNoOtherCalls();
    }

    [Fact(Skip = "Todo: do after refactoring ai Chat for queue")]
    public async Task HandleAsync_ProcessingQuestionAi_MissingThreadId_ThrowsNonRetryable_AndSkipsWork()
    {
        var (engine, ai, pub, accessorClient, log, sut) = CreateSut();

        var requestId = Guid.NewGuid().ToString();
        var userId = "testUserId";

        var req = new EngineChatRequest
        {
            RequestId = requestId,
            ThreadId = Guid.Empty,
            UserMessage = "hello",
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TtlSeconds = 120,
        };

        accessorClient.Setup(ac => ac.GetChatHistoryAsync(Guid.Empty, It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("Invalid ThreadId"));

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingQuestionAi,
            Payload = ToJsonElement(req)
        };

        var act = () => sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<RetryableException>()
          .WithMessage("*Transient error while processing AI question*");

        accessorClient.Verify(ac => ac.GetChatHistoryAsync(Guid.Empty, It.IsAny<CancellationToken>()), Times.Once);


        // no dependencies were invoked
        accessorClient.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        engine.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ProcessingQuestionAi_AiThrows_WrappedInRetryable()
    {
        var (engine, ai, pub, accessorClient, log, sut) = CreateSut();


        var requestId = Guid.NewGuid().ToString();
        var threadId = Guid.NewGuid();
        var userId = "testUserId";

        var chatHistory = new List<ChatMessage>();

        var requestToEngine = new EngineChatRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserMessage = "boom",
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            TtlSeconds = 120,
        };

        var expectedAiRequest = new ChatAiServiseRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserMessage = "boom",
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = requestToEngine.SentAt,
            TtlSeconds = 120,
            History = chatHistory
        };

        accessorClient.Setup(ac => ac.GetChatHistoryAsync(threadId, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(chatHistory);

        ai.Setup(a => a.ChatHandlerAsync(expectedAiRequest, It.IsAny<CancellationToken>()))
          .ThrowsAsync(new InvalidOperationException("AI failed"));


        var msg = new Message
        {
            ActionName = MessageAction.ProcessingQuestionAi,
            Payload = ToJsonElement(requestToEngine)
        };

        var act = () => sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<RetryableException>()
                           .WithMessage("*Transient error while processing AI question*"))
                 .Which;

        ex.InnerException.Should().BeOfType<InvalidOperationException>();
        ex.InnerException!.Message.Should().Contain("AI failed");

        accessorClient.Verify(ac => ac.GetChatHistoryAsync(threadId, It.IsAny<CancellationToken>()), Times.Once);
        ai.Verify(a => a.ChatHandlerAsync(expectedAiRequest, It.IsAny<CancellationToken>()), Times.Once);
        pub.VerifyNoOtherCalls();
        engine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_UnknownAction_ThrowsNonRetryable_AndSkipsWork()
    {
        var (engine, ai, pub, accessorClient, log, sut) = CreateSut();

        var msg = new Message
        {
            ActionName = (MessageAction)9999,
            Payload = JsonSerializer.Deserialize<JsonElement>("{}")
        };

        var act = () => sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*No handler for action 9999*");

        engine.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }
}
