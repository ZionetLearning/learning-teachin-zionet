using DotQueue;
using Engine.Constants;
using Engine.Endpoints;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Dapr.Client;

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

    // Centralized SUT + mocks factory
    private static (
        Mock<IEngineService> engine,
        Mock<IChatAiService> ai,
        Mock<IAiReplyPublisher> pub,
        Mock<IAccessorClient> accessorClient,
        Mock<ILogger<EngineQueueHandler>> log,
        Mock<DaprClient> dapr,
        EngineQueueHandler sut
    ) CreateSut()
    {
        var engine = new Mock<IEngineService>(MockBehavior.Strict);
        var ai = new Mock<IChatAiService>(MockBehavior.Strict);
        var pub = new Mock<IAiReplyPublisher>(MockBehavior.Strict);
        var accessorClient = new Mock<IAccessorClient>(MockBehavior.Strict);
        var log = new Mock<ILogger<EngineQueueHandler>>();
        var dapr = new Mock<DaprClient>();

        var sut = new EngineQueueHandler(
            engine.Object,
            ai.Object,
            pub.Object,
            accessorClient.Object,
            log.Object,
            dapr.Object
        );

        return (engine, ai, pub, accessorClient, log, dapr, sut);
    }

    [Fact]
    public async Task HandleAsync_CreateTask_Processes_TaskModel()
    {
        var (engine, ai, pub, accessorClient, log, dapr, sut) = CreateSut();

        var task = new TaskModel { Id = 42, Name = "demo", Payload = "{}" };

        engine.Setup(e => e.ProcessTaskAsync(task, It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

        var msg = new Message
        {
            ActionName = MessageAction.CreateTask,
            Payload = ToJsonElement(task)
        };

        await sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        engine.Verify(e => e.ProcessTaskAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        engine.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_CreateTask_InvalidPayload_DoesNotCall_Engine()
    {
        var (engine, ai, pub, accessorClient, log, dapr, sut) = CreateSut();

        var msg = new Message
        {
            ActionName = MessageAction.CreateTask,
            Payload = JsonSerializer.Deserialize<JsonElement>("null")
        };

        var act = () => sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*Payload deserialization returned null*");

        engine.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    [Fact(Skip = "Todo: do after refactoring ai Chat for queue")]
    public async Task HandleAsync_ProcessingChatMessage_HappyPath_Calls_Ai_And_Publishes()
    {
        var (engine, ai, pub, accessorClient, log, dapr, sut) = CreateSut();

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

        var expectedAiReq = new ChatAiServiseRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserMessage = userMsg,
            ChatType = ChatType.Default,
            UserId = userId,
            SentAt = engineReq.SentAt,
            TtlSeconds = 120,
            History = snapshotFromAccessor.History
        };

        var updatedHistory = Je("""
        {
          "messages":[
            {"role":"system","content":"..."},
            {"role":"user","content":"hello"},
            {"role":"assistant","content":"world"}
          ]
        }
        """);

        var answer = new ChatMessage
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            UserId = userId,
            Role = MessageRole.Assistant,
            Content = textAnswer
        };

        var aiResp = new ChatAiServiceResponse
        {
            RequestId = requestId,
            ThreadId = threadId,
            Status = ChatAnswerStatus.Ok,
            Answer = answer,
            Name = chatName,
            UpdatedHistory = updatedHistory
        };

        ai.Setup(a => a.ChatHandlerAsync(It.Is<ChatAiServiseRequest>(r =>
                        r.RequestId == expectedAiReq.RequestId &&
                        r.ThreadId == expectedAiReq.ThreadId &&
                        r.UserMessage == expectedAiReq.UserMessage &&
                        r.UserId == expectedAiReq.UserId &&
                        r.ChatType == expectedAiReq.ChatType &&
                        r.History.GetRawText() == expectedAiReq.History.GetRawText()
                    ), It.IsAny<CancellationToken>()))
          .ReturnsAsync(aiResp);

        accessorClient.Setup(a => a.UpsertHistorySnapshotAsync(
                        It.Is<UpsertHistoryRequest>(u =>
                            u.ThreadId == threadId &&
                            u.UserId == userId &&
                            u.ChatType == "default" &&
                            u.History.GetRawText() == updatedHistory.GetRawText()
                        ),
                        It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new HistorySnapshotDto
                      {
                          ThreadId = threadId,
                          UserId = userId,
                          ChatType = "default",
                          History = updatedHistory
                      });

        var engineResponse = new EngineChatResponse
        {
            RequestId = requestId,
            AssistantMessage = textAnswer,
            ChatName = chatName,
            ThreadId = threadId,
            Status = ChatAnswerStatus.Ok
        };

        var chatMetadata = new UserContextMetadata { UserId = userId.ToString() };

        pub.Setup(p => p.SendReplyAsync(chatMetadata, engineResponse, It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingChatMessage,
            Payload = ToJsonElement(engineReq)
        };

        await sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        accessorClient.Verify(a => a.GetHistorySnapshotAsync(threadId, userId, It.IsAny<CancellationToken>()), Times.Once);
        ai.VerifyAll();
        accessorClient.Verify(a => a.UpsertHistorySnapshotAsync(It.IsAny<UpsertHistoryRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        pub.Verify(p => p.SendReplyAsync(chatMetadata, engineResponse, It.IsAny<CancellationToken>()), Times.Once);

        engine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_UnknownAction_ThrowsNonRetryable()
    {
        var (engine, ai, pub, accessorClient, log, dapr, sut) = CreateSut();

        var msg = new Message
        {
            ActionName = (MessageAction)9999,
            Payload = JsonSerializer.Deserialize<JsonElement>("{}")
        };

        var act = () => sut.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*No handler for action 9999*");

        engine.VerifyNoOtherCalls();
        ai.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }
}