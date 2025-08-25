using Manager.Hubs;
using Manager.Models;
using Manager.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace ManagerUnitTests.Services;

public class NotificationServiceTests
{
    private static (
        Mock<INotificationClient> userClientMock,
        Mock<IHubContext<NotificationHub, INotificationClient>> hubContextMock,
        NotificationService service
    ) CreateNotificationService()
    {
        var logger = new Mock<ILogger<NotificationService>>();
        var hubContextMock = new Mock<IHubContext<NotificationHub, INotificationClient>>();
        var userClientMock = new Mock<INotificationClient>(MockBehavior.Strict);

        // Setup hub context to return our mock client for user-specific calls
        hubContextMock.Setup(h => h.Clients.User(It.IsAny<string>()))
            .Returns(userClientMock.Object);

        var service = new NotificationService(hubContextMock.Object, logger.Object);
        return (userClientMock, hubContextMock, service);
    }

    [Theory]
    [InlineData("TaskUpdated", EventType.TaskUpdate, 7, "RUNNING")]
    [InlineData("NotificationReceived", null, "hi")]
    public async Task SendsCorrectMessage_ToUser(string signalMethod, EventType? eventType, params object[] args)
    {
        var (userClientMock, hubContextMock, service) = CreateNotificationService();
        var userId = "testUser";

        if (signalMethod == "NotificationReceived")
        {
            // Setup for notification
            var notification = new UserNotification
            {
                Message = (string)args[0],
                Type = NotificationType.Info,
                Timestamp = DateTimeOffset.UtcNow
            };

            userClientMock.Setup(c => c.NotificationMessage(It.Is<UserNotification>(n => n.Message == notification.Message)))
                .Returns(Task.CompletedTask);

            // Act
            await service.SendNotificationAsync(userId, notification);

            // Assert
            userClientMock.Verify(c => c.NotificationMessage(It.Is<UserNotification>(n => n.Message == notification.Message)), Times.Once);
        }
        else if (signalMethod == "TaskUpdated" && eventType.HasValue)
        {
            // Setup for task update event
            var payload = new TaskUpdateMessage
            {
                TaskId = (int)args[0],
                Status = (string)args[1]
            };

            userClientMock.Setup(c => c.ReceiveEvent(It.Is<UserEvent<JsonElement>>(evt =>
                evt.EventType == eventType.Value)))
                .Returns(Task.CompletedTask);

            // Act
            await service.SendEventAsync(eventType.Value, userId, payload);

            // Assert
            userClientMock.Verify(c => c.ReceiveEvent(It.Is<UserEvent<JsonElement>>(evt =>
                evt.EventType == eventType.Value)), Times.Once);
        }

        hubContextMock.Verify(h => h.Clients.User(userId), Times.Once);
    }

    [Fact]
    public async Task SendNotificationAsync_SendsNotificationToSpecificUser()
    {
        // Arrange
        var (userClientMock, hubContextMock, service) = CreateNotificationService();
        var userId = "user123";
        var notification = new UserNotification
        {
            Message = "Test notification",
            Type = NotificationType.Warning,
            Timestamp = DateTimeOffset.UtcNow
        };

        userClientMock.Setup(c => c.NotificationMessage(notification))
            .Returns(Task.CompletedTask);

        // Act
        await service.SendNotificationAsync(userId, notification);

        // Assert
        hubContextMock.Verify(h => h.Clients.User(userId), Times.Once);
        userClientMock.Verify(c => c.NotificationMessage(notification), Times.Once);
    }

    [Fact]
    public async Task SendEventAsync_WithTaskUpdate_SendsEventToSpecificUser()
    {
        // Arrange
        var (userClientMock, hubContextMock, service) = CreateNotificationService();
        var userId = "user456";
        var payload = new TaskUpdateMessage { TaskId = 42, Status = "COMPLETED" };

        userClientMock.Setup(c => c.ReceiveEvent(It.Is<UserEvent<JsonElement>>(evt =>
            evt.EventType == EventType.TaskUpdate)))
            .Returns(Task.CompletedTask);

        // Act
        await service.SendEventAsync(EventType.TaskUpdate, userId, payload);

        // Assert
        hubContextMock.Verify(h => h.Clients.User(userId), Times.Once);
        userClientMock.Verify(c => c.ReceiveEvent(It.Is<UserEvent<JsonElement>>(evt =>
            evt.EventType == EventType.TaskUpdate)), Times.Once);
    }

    [Fact]
    public async Task SendEventAsync_WithChatAiAnswer_SendsEventToSpecificUser()
    {
        // Arrange
        var (userClientMock, hubContextMock, service) = CreateNotificationService();
        var userId = "user789";
        var payload = new { Answer = "AI response", RequestId = "req123" };

        userClientMock.Setup(c => c.ReceiveEvent(It.Is<UserEvent<JsonElement>>(evt =>
            evt.EventType == EventType.ChatAiAnswer)))
            .Returns(Task.CompletedTask);

        // Act
        await service.SendEventAsync(EventType.ChatAiAnswer, userId, payload);

        // Assert
        hubContextMock.Verify(h => h.Clients.User(userId), Times.Once);
        userClientMock.Verify(c => c.ReceiveEvent(It.Is<UserEvent<JsonElement>>(evt =>
            evt.EventType == EventType.ChatAiAnswer)), Times.Once);
    }

    [Fact]
    public async Task SendNotificationAsync_ThrowsIfUserIdIsEmpty()
    {
        // Arrange
        var (_, _, service) = CreateNotificationService();
        var notification = new UserNotification
        {
            Message = "Test notification",
            Type = NotificationType.Info,
            Timestamp = DateTimeOffset.UtcNow
        };
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendNotificationAsync(string.Empty, notification));
    }

    [Fact]
    public async Task SendNotificationAsync_ThrowsIfUserIdIsNull()
    {
        // Arrange
        var (_, _, service) = CreateNotificationService();
        var notification = new UserNotification
        {
            Message = "Test notification",
            Type = NotificationType.Info,
            Timestamp = DateTimeOffset.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendNotificationAsync(null!, notification));
    }

    [Fact]
    public async Task SendEventAsync_ThrowsIfUserIdIsEmpty()
    {
        // Arrange
        var (_, _, service) = CreateNotificationService();
        var payload = new TaskUpdateMessage { TaskId = 1, Status = "RUNNING" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendEventAsync(EventType.TaskUpdate, string.Empty, payload));
    }

    [Fact]
    public async Task SendEventAsync_ThrowsIfUserIdIsNull()
    {
        // Arrange
        var (_, _, service) = CreateNotificationService();
        var payload = new TaskUpdateMessage { TaskId = 1, Status = "RUNNING" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.SendEventAsync(EventType.TaskUpdate, null!, payload));
    }
}
