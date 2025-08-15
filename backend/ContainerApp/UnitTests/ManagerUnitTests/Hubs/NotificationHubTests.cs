using System.Reflection;
using Manager.Hubs;
using Manager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Manager.UnitTests.Hubs;

public class NotificationHubTests
{
    private static (
        Mock<INotificationClient> clientMock,
        NotificationHub hub
    ) CreateHub(string signalMethod, Type expectedPayloadType)
    {
        var logger = new Mock<ILogger<NotificationHub>>();
        var clients = new Mock<IHubCallerClients<INotificationClient>>();
        var clientMock = new Mock<INotificationClient>(MockBehavior.Strict);

        clients.Setup(c => c.All).Returns(clientMock.Object);

        // Setup the expected method call on INotificationClient
        if (signalMethod == "NotificationReceived")
        {
            clientMock.Setup(c => c.NotificationMessage(It.IsAny<UserNotification>()))
                .Returns(Task.CompletedTask);
        }
        else if (signalMethod == "TaskUpdated")
        {
            clientMock.Setup(c => c.ReceiveEvent(It.IsAny<UserEvent<TaskUpdateMessage>>()))
            .Returns(Task.CompletedTask);
        }

        var hub = new NotificationHub(logger.Object) { Clients = clients.Object };
        return (clientMock, hub);
    }

    [Theory]
    [InlineData("TaskUpdated", typeof(TaskUpdateMessage), "SendTaskUpdate", 7, "RUNNING")]
    [InlineData("NotificationReceived", typeof(NotificationMessage), "SendNotification", "hi")]
    public async Task Broadcasts_To_All(string signalMethod, Type payloadType, string hubMethod, params object[] args)
    {
        var (clientMock, hub) = CreateHub(signalMethod, payloadType);

        var mi = typeof(NotificationHub).GetMethod(hubMethod, BindingFlags.Instance | BindingFlags.Public);
        Assert.NotNull(mi);

        var result = mi!.Invoke(hub, args);
        if (result is Task task)
        {
            await task;
        }
        else
        {
            Assert.Fail($"Method {hubMethod} did not return a Task.");
        }

        // Assert
        clientMock.VerifyAll();
    }
}
