using System.Text.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Services;
using DotQueue;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Dapr.Client;

namespace AccessorUnitTests.Endpoints;

public class AccessorQueueHandlerTests
{
    private static Mock<IAccessorService> Svc() => new(MockBehavior.Strict);
    private static Mock<ILogger<AccessorQueueHandler>> Log() => new();
    private static Mock<IManagerCallbackQueueService> Publisher() => new(MockBehavior.Strict);
    private static Mock<DaprClient> Dapr() => new(MockBehavior.Strict);

    private static Message MakeMessage(MessageAction action, object payload)
        => new Message
        {
            ActionName = action,
            Payload = JsonSerializer.SerializeToElement(payload)
        };

    [Fact]
    public async Task HandleAsync_UpdateTask_Calls_Service()
    {
        var svc = Svc();
        var pub = Publisher();
        var log = Log();
        var dapr = Dapr();

        var handler = new AccessorQueueHandler(svc.Object, pub.Object, log.Object, dapr.Object);

        var payload = new TaskModel { Id = 10, Name = "new-name" };
        var msg = MakeMessage(MessageAction.UpdateTask, payload);

        svc.Setup(s => s.UpdateTaskNameAsync(10, "new-name"))
           .ReturnsAsync(true);

        var renewed = false;
        Func<Task> renew = () => { renewed = true; return Task.CompletedTask; };

        await handler.HandleAsync(msg, metadataCallback: null, renew, CancellationToken.None);

        renewed.Should().BeFalse(); // we didn't call renew
        svc.VerifyAll();
        dapr.VerifyNoOtherCalls();
    }

    [Theory]
    [MemberData(nameof(InvalidMessages))]
    public async Task HandleAsync_InvalidMessage_Throws_And_NoServiceCalls(Message msg, string expectedMessagePart)
    {
        var svc = Svc();
        var pub = Publisher();
        var log = Log();
        var dapr = Dapr();

        var handler = new AccessorQueueHandler(svc.Object, pub.Object, log.Object, dapr.Object);

        Func<Task> act = () => handler.HandleAsync(msg, metadataCallback: null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage($"*{expectedMessagePart}*");

        svc.VerifyNoOtherCalls();
        dapr.VerifyNoOtherCalls();
    }

    public static IEnumerable<object[]> InvalidMessages()
    {
        yield return new object[]
        {
            new Message { ActionName = (MessageAction)9999, Payload = JsonDocument.Parse("{}").RootElement },
            "No handler for action"
        };
        yield return new object[]
        {
            new Message { ActionName = MessageAction.UpdateTask, Payload = JsonDocument.Parse("null").RootElement },
            "deserialization returned null"
        };
    }

    [Fact]
    public async Task HandleAsync_UpdateTask_InvalidPayload_Throws_NonRetryable_And_NoServiceCalls()
    {
        var svc = Svc();
        var pub = Publisher();
        var log = Log();
        var dapr = Dapr();

        var handler = new AccessorQueueHandler(svc.Object, pub.Object, log.Object, dapr.Object);

        var jsonNull = JsonDocument.Parse("null").RootElement;

        var msg = new Message { ActionName = MessageAction.UpdateTask, Payload = jsonNull };

        Func<Task> act = () => handler.HandleAsync(msg, metadataCallback: null, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*deserialization returned null*TaskModel*");

        svc.VerifyNoOtherCalls();
        dapr.VerifyNoOtherCalls();
    }
}