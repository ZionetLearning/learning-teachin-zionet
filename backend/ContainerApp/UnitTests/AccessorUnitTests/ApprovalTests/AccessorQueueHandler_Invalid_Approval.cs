using System.Text.Json;
using Newtonsoft.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.ApprovalTests;

public class AccessorQueueHandler_Invalid_Approval
{
    [Theory]
    [MemberData(nameof(BadMessages))]
    public async Task Invalid_Message_Shapes_ErrorContract_Is_Approved(Message msg, string tag)
    {
        var svc = new Mock<IAccessorService>(MockBehavior.Strict);
        var log = new Mock<ILogger<AccessorQueueHandler>>();
        var handler = new AccessorQueueHandler(svc.Object, log.Object);

        Exception? ex = null;
        try
        {
            await handler.HandleAsync(msg, () => Task.CompletedTask, CancellationToken.None);
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

        // Use your scrubbed approval helper
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
