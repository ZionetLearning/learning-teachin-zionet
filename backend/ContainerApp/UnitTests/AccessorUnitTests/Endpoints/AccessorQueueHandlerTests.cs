using System.Text.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using DotQueue;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Endpoints;

public class AccessorQueueHandlerTests
{
    private static Mock<IAccessorService> Svc() => new(MockBehavior.Strict);
    private static Mock<ILogger<AccessorQueueHandler>> Log() => new();

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
        var log = Log();
        var handler = new AccessorQueueHandler(svc.Object, log.Object);

        var payload = new TaskModel { Id = 10, Name = "new-name" };
        var msg = MakeMessage(MessageAction.UpdateTask, payload);

        svc.Setup(s => s.UpdateTaskNameAsync(10, "new-name")).ReturnsAsync(true);

        var renewed = false;
        Func<Task> renew = () => { renewed = true; return Task.CompletedTask; };

        await handler.HandleAsync(msg, renew, CancellationToken.None);

        renewed.Should().BeFalse(); // current handler doesn't use renew; assert we didn't accidentally call it
        svc.VerifyAll();
    }
    [Theory]
    [MemberData(nameof(InvalidMessages))]
    public async Task HandleAsync_InvalidMessage_Throws_And_NoServiceCalls(Message msg, string expectedMessagePart)
    {
        // Arrange
        var svc = Svc();
        var log = Log();
        var handler = new AccessorQueueHandler(svc.Object, log.Object);

        // Act
        Func<Task> act = () => handler.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage($"*{expectedMessagePart}*");

        svc.VerifyNoOtherCalls();
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
            "No handler for action"
        };
        yield return new object[]
        {
            new Message
            {
                ActionName = MessageAction.UpdateTask,
                Payload = JsonDocument.Parse("null").RootElement
            },
            "deserialization returned null"
        };
    }

    [Fact]
    public async Task HandleAsync_UpdateTask_InvalidPayload_Throws_NonRetryable_And_NoServiceCalls()
    {
        var svc = Svc();
        var log = Log();
        var handler = new AccessorQueueHandler(svc.Object, log.Object);

        // JSON 'null' forces Deserialize<T> to yield null -> your code throws NonRetryableException
        var jsonNull = JsonDocument.Parse("null").RootElement;

        var msg = new Message
        {
            ActionName = MessageAction.UpdateTask,
            Payload = jsonNull
        };

        Func<Task> act = () => handler.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*deserialization returned null*TaskModel*"); // matches "Payload deserialization returned null for TaskModel."

        // ensure no service calls were made
        svc.VerifyNoOtherCalls();
    }
}
