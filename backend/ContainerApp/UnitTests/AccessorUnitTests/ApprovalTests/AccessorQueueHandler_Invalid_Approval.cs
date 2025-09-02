using System.Text.Json;
using Newtonsoft.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Models.QueueMessages;
using Accessor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Dapr.Client;
using Accessor.Routing;

namespace AccessorUnitTests.ApprovalTests;

public class AccessorQueueHandler_Invalid_Approval
{
    private static RoutingMiddleware Routing()
    {
        var accessor = new Mock<IRoutingContextAccessor>();
        var logger = new Mock<ILogger<RoutingMiddleware>>();
        return new RoutingMiddleware(accessor.Object, logger.Object);
    }

    [Theory]
    [MemberData(nameof(BadMessages))]
    public async Task Invalid_Message_Shapes_ErrorContract_Is_Approved(Message msg, string tag)
    {
        var svc = new Mock<IAccessorService>(MockBehavior.Strict);
        var log = new Mock<ILogger<AccessorQueueHandler>>();
        var managerCallbackSvc = new Mock<IManagerCallbackQueueService>(MockBehavior.Strict);
        var dapr = new Mock<DaprClient>();
        var dispatcher = new Mock<IQueueDispatcher>(MockBehavior.Strict);
        var routing = Routing();

        var handler = new AccessorQueueHandler(svc.Object, managerCallbackSvc.Object, log.Object, dapr.Object, dispatcher.Object, routing);

        Exception? ex = null;
        try
        {
            await handler.HandleAsync(msg, null, () => Task.CompletedTask, CancellationToken.None);
        }
        catch (Exception e)
        {
            ex = e;
        }

        Assert.NotNull(ex);

        var snapshot = new
        {
            caseTag = tag,
            exceptionType = ex!.GetType().FullName,
            message = ex.Message
        };

        ApprovalSetup.VerifyJsonClean(
            JsonConvert.SerializeObject(snapshot, Formatting.Indented),
            additionalInfo: $"QueueHandler_{tag}"
        );

        svc.VerifyNoOtherCalls();
    }

    public static IEnumerable<object[]> BadMessages()
    {
        yield return new object[] {
            new Message { ActionName = (MessageAction)9999, Payload = JsonDocument.Parse("{}").RootElement },
            "UnknownAction"
        };
        yield return new object[] {
            new Message { ActionName = MessageAction.UpdateTask, Payload = JsonDocument.Parse("null").RootElement },
            "NullPayload"
        };
        yield return new object[] {
            new Message { ActionName = MessageAction.UpdateTask, Payload = System.Text.Json.JsonSerializer.SerializeToElement(new TaskModel { Id = 0, Name = "x" }) },
            "InvalidId"
        };
        yield return new object[] {
            new Message { ActionName = MessageAction.UpdateTask, Payload = System.Text.Json.JsonSerializer.SerializeToElement(new TaskModel { Id = 1, Name = "   " }) },
            "MissingName"
        };
    }
}