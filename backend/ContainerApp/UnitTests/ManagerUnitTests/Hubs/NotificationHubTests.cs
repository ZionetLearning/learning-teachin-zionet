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
        Mock<IClientProxy> proxy,
        NotificationHub hub
    ) CreateHub(string signalMethod, Type expectedPayloadType)
    {
        var logger = new Mock<ILogger<NotificationHub>>();
        var clients = new Mock<IHubCallerClients>();
        var proxy = new Mock<IClientProxy>(MockBehavior.Strict);

        clients.Setup(c => c.All).Returns(proxy.Object);

        proxy.Setup(p => p.SendCoreAsync(
                signalMethod,
                It.Is<object[]>(args => args.Length == 1 && expectedPayloadType.IsInstanceOfType(args[0])),
                default))
            .Returns(Task.CompletedTask);

        var hub = new NotificationHub(logger.Object) { Clients = clients.Object };
        return (proxy, hub);
    }

    [Theory]
    [InlineData("TaskUpdated", typeof(TaskUpdateMessage), "SendTaskUpdate", 7, "RUNNING")]
    [InlineData("NotificationReceived", typeof(NotificationMessage), "SendNotification", "hi")]
    public async Task Broadcasts_To_All(string signalMethod, Type payloadType, string hubMethod, params object[] args)
    {
        var (proxy, hub) = CreateHub(signalMethod, payloadType);

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
        proxy.VerifyAll();
    }
}
