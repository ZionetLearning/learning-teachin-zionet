using System.Text.Json;
using Accessor.Endpoints;
using Accessor.Messaging;
using Accessor.Models;
using Accessor.Services;
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
    [Fact]
    public async Task HandleAsync_UnknownAction_Throws_NonRetryable_And_NoServiceCalls()
    {
        var svc = Svc();
        var log = Log();
        var handler = new AccessorQueueHandler(svc.Object, log.Object);

        var msg = MakeMessage((MessageAction)9999, new { foo = "bar" });

        Func<Task> act = () => handler.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);

        await act.Should().ThrowAsync<NonRetryableException>()
                 .WithMessage("*No handler for action*9999*"); // wildcard for stability

        // ensure handler didn’t touch the service layer
        svc.VerifyNoOtherCalls();
    }

    // invalid payload => production currently throws NonRetryableException from DeserializeOrThrow<T>
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
