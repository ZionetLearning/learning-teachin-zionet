using System.Text.Json;
using Engine.Constants;
using Engine.Endpoints;
using Engine.Messaging;
using Engine.Models;
using Engine.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class EngineQueueHandlerTests
{
    private static JsonElement ToJsonElement<T>(T obj) =>
        JsonSerializer.SerializeToElement(obj);

    // Centralized SUT + mocks factory to reduce boilerplate
    private static (
        Mock<IEngineService> engine,
        Mock<IChatAiService> ai,
        Mock<IAiReplyPublisher> pub,
        Mock<ILogger<EngineQueueHandler>> log,
        EngineQueueHandler sut
    ) CreateSut()
    {
        var engine = new Mock<IEngineService>(MockBehavior.Strict);
        var ai = new Mock<IChatAiService>(MockBehavior.Strict);
        var pub = new Mock<IAiReplyPublisher>(MockBehavior.Strict);
        var log = new Mock<ILogger<EngineQueueHandler>>();
        var sut = new EngineQueueHandler(engine.Object, ai.Object, pub.Object, log.Object);
        return (engine, ai, pub, log, sut);
    }

    [Fact]
    public async Task HandleAsync_CreateTask_Processes_TaskModel()
    {
        // Arrange
        var (engine, ai, pub, log, sut) = CreateSut();

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
        var (engine, ai, pub, log, sut) = CreateSut();

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
        var (engine, ai, pub, log, sut) = CreateSut();

        var req = new AiRequestModel
        {
            Id = "id-1",
            ThreadId = "thread-1",
            Question = "hello",
            ReplyToQueue = ""
        };

        var aiResponse = new AiResponseModel
        {
            Id = "id-1",
            ThreadId = "thread-1",
            Status = "ok",
            Answer = "world",
            Error = null
        };

        ai.Setup(a => a.ProcessAsync(req, It.IsAny<CancellationToken>()))
          .ReturnsAsync(aiResponse);

        pub.Setup(p => p.SendReplyAsync(aiResponse, $"{QueueNames.ManagerCallbackQueue}-out", It.IsAny<CancellationToken>()))
           .Returns(Task.CompletedTask);

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingQuestionAi,
            Payload = ToJsonElement(req)
        };

        // Act
        await sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        ai.Verify(a => a.ProcessAsync(req, It.IsAny<CancellationToken>()), Times.Once);
        ai.VerifyNoOtherCalls();
        pub.Verify(p => p.SendReplyAsync(aiResponse, $"{QueueNames.ManagerCallbackQueue}-out", It.IsAny<CancellationToken>()), Times.Once);
        pub.VerifyNoOtherCalls();
        engine.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ProcessingQuestionAi_MissingThreadId_ThrowsNonRetryable_AndSkipsWork()
    {
        var (engine, ai, pub, log, sut) = CreateSut();

        var req = new AiRequestModel
        {
            Id = "id-1",
            ThreadId = "", // invalid
            Question = "hello",
            ReplyToQueue = ""
        };

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingQuestionAi,
            Payload = ToJsonElement(req)
        };

        var act = () => sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*ThreadId is required*");

        // no dependencies were invoked
        ai.Verify(a => a.ProcessAsync(It.IsAny<AiRequestModel>(), It.IsAny<CancellationToken>()), Times.Never);
        engine.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task HandleAsync_ProcessingQuestionAi_AiThrows_WrappedInRetryable()
    {
        var (engine, ai, pub, log, sut) = CreateSut();

        var req = new AiRequestModel
        {
            Id = "id-err",
            ThreadId = "t",
            Question = "boom",
            ReplyToQueue = ""
        };

        ai.Setup(a => a.ProcessAsync(req, It.IsAny<CancellationToken>()))
          .ThrowsAsync(new InvalidOperationException("AI failed"));

        var msg = new Message
        {
            ActionName = MessageAction.ProcessingQuestionAi,
            Payload = ToJsonElement(req)
        };

        var act = () => sut.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<RetryableException>()
                           .WithMessage("*Transient error while processing AI question*"))
                 .Which;

        ex.InnerException.Should().BeOfType<InvalidOperationException>();
        ex.InnerException!.Message.Should().Contain("AI failed");

        pub.VerifyNoOtherCalls();
        engine.VerifyNoOtherCalls();
        ai.Verify(a => a.ProcessAsync(req, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_UnknownAction_ThrowsNonRetryable_AndSkipsWork()
    {
        var (engine, ai, pub, log, sut) = CreateSut();

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
