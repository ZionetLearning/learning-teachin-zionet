using System.Text.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Services.Interfaces;
using DotQueue;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Endpoints;

public class AccessorQueueHandlerTests
{
    private static Mock<ITaskService> TaskSvc() => new(MockBehavior.Strict);
    private static Mock<IManagerCallbackQueueService> Publisher() => new(MockBehavior.Strict);
    private static Mock<ILogger<AccessorQueueHandler>> Log() => new();

    private static Message MakeMessage(MessageAction action, object payload)
        => new Message
        {
            ActionName = action,
            Payload = JsonSerializer.SerializeToElement(payload)
        };

    [Fact]
    public async Task HandleAsync_UpdateTask_Calls_TaskService()
    {
        var taskSvc = TaskSvc();
        var pub = Publisher();
        var log = Log();
        var handler = new AccessorQueueHandler(taskSvc.Object, pub.Object, log.Object);

        var payload = new TaskModel { Id = 10, Name = "new-name" };
        var msg = MakeMessage(MessageAction.UpdateTask, payload);

        taskSvc.Setup(s => s.UpdateTaskNameAsync(10, "new-name", null))
              .ReturnsAsync(new UpdateTaskResult(
                  Updated: true,
                  NotFound: false,
                  PreconditionFailed: false,
                  NewEtag: "456"
              ));


        var renewed = false;
        Func<Task> renew = () => { renewed = true; return Task.CompletedTask; };

        await handler.HandleAsync(msg, null, renew, CancellationToken.None);

        renewed.Should().BeFalse(); // ensure renew lock not called
        taskSvc.VerifyAll();
        pub.VerifyNoOtherCalls();
    }

    [Theory]
    [MemberData(nameof(InvalidMessages))]
    public async Task HandleAsync_InvalidMessage_Throws_And_NoServiceCalls(Message msg, string expectedMessagePart)
    {
        var taskSvc = TaskSvc();
        var pub = Publisher();
        var log = Log();
        var handler = new AccessorQueueHandler(taskSvc.Object, pub.Object, log.Object);

        Func<Task> act = () => handler.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage($"*{expectedMessagePart}*");

        taskSvc.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }

    public static IEnumerable<object[]> InvalidMessages()
    {
        yield return new object[]
        {
            new Message
            {
                ActionName = (MessageAction)9999,
                Payload = JsonDocument.Parse("{}").RootElement
            },
            "No handler registered for action"
        };
        yield return new object[]
        {
            new Message
            {
                ActionName = MessageAction.UpdateTask,
                Payload = JsonDocument.Parse("null").RootElement
            },
            "Payload deserialization returned null"
        };
    }

    [Fact]
    public async Task HandleAsync_UpdateTask_InvalidPayload_Throws_NonRetryable_And_NoServiceCalls()
    {
        var taskSvc = TaskSvc();
        var pub = Publisher();
        var log = Log();
        var handler = new AccessorQueueHandler(taskSvc.Object, pub.Object, log.Object);

        var msg = new Message
        {
            ActionName = MessageAction.UpdateTask,
            Payload = JsonDocument.Parse("null").RootElement
        };

        Func<Task> act = () => handler.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*Payload deserialization returned null*TaskModel*");

        taskSvc.VerifyNoOtherCalls();
        pub.VerifyNoOtherCalls();
    }
}