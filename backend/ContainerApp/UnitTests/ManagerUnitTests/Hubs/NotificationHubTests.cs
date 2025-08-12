using Manager.Hubs;
using Manager.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Manager.UnitTests.Hubs;

public class NotificationHubTests
{
    [Fact]
    public async Task SendTaskUpdate_Broadcasts()
    {
        var logger = new Mock<ILogger<NotificationHub>>();
        var clients = new Mock<IHubCallerClients>();
        var proxy = new Mock<IClientProxy>();

        clients.Setup(c => c.All).Returns(proxy.Object);
        proxy.Setup(p => p.SendCoreAsync(
                "TaskUpdated",
                It.Is<object[]>(args => args.Length == 1 && args[0] is TaskUpdateMessage),
                default))
            .Returns(Task.CompletedTask);

        var hub = new NotificationHub(logger.Object) { Clients = clients.Object };

        await hub.SendTaskUpdate(7, "RUNNING");

        proxy.VerifyAll();
    }

    [Fact]
    public async Task SendNotification_Broadcasts()
    {
        var logger = new Mock<ILogger<NotificationHub>>();
        var clients = new Mock<IHubCallerClients>();
        var proxy = new Mock<IClientProxy>();

        clients.Setup(c => c.All).Returns(proxy.Object);
        proxy.Setup(p => p.SendCoreAsync(
                "NotificationReceived",
                It.Is<object[]>(args => args.Length == 1 && args[0] is NotificationMessage),
                default))
            .Returns(Task.CompletedTask);

        var hub = new NotificationHub(logger.Object) { Clients = clients.Object };

        await hub.SendNotification("hi");

        proxy.VerifyAll();
    }
}
