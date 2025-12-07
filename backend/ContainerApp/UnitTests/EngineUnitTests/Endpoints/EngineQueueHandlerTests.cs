using Dapr.Client;
using DotQueue;
using Engine.Endpoints;
using Engine.Helpers;
using Engine.Models;
using Engine.Models.Chat;
using Engine.Models.QueueMessages;
using Engine.Services;
using Engine.Services.Clients.AccessorClient;
using Engine.Services.Clients.AccessorClient.Models;
using FluentAssertions;
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