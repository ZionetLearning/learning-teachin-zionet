using Accessor.Endpoints;
using Microsoft.AspNetCore.Http;
using Moq;
using Accessor.Models;
using Accessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AccessorUnitTests;

public class AccessorEndpointsTests
{
    private readonly Mock<IAccessorService> _mockService;
    private readonly Mock<ILogger<AccessorService>> _mockLogger;

    public AccessorEndpointsTests()
    {
        _mockService = new Mock<IAccessorService>();
        _mockLogger = new Mock<ILogger<AccessorService>>();
    }

    [Fact]
    public async Task GetTaskById_ReturnsOk_WhenTaskExists()
    {
        // Arrange
        var task = new TaskModel { Id = 1, Name = "Test" };
        var context = new DefaultHttpContext();
        var request = context.Request;

        _mockService.Setup(s => s.GetTaskByIdAsync(1, It.IsAny<IDictionary<string, string>>()))
                    .ReturnsAsync(task);

        // Act
        var result = await AccessorEndpoints.GetTaskByIdAsync(
            1,
            request,
            _mockService.Object,
            _mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<TaskModel>>(result);
        Assert.Equal(1, okResult.Value?.Id);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = 999;
        var context = new DefaultHttpContext();
        var request = context.Request;

        _mockService.Setup(s => s.GetTaskByIdAsync(taskId, It.IsAny<IDictionary<string, string>>()))
                    .ReturnsAsync((TaskModel?)null);

        // Act
        var result = await AccessorEndpoints.GetTaskByIdAsync(
            taskId,
            request,
            _mockService.Object,
            _mockLogger.Object);

        // Assert
        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, statusResult.StatusCode);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var value = valueResult.Value;

        Assert.NotNull(value);
        Assert.Equal($"Task with ID {taskId} not found.", value?.ToString());
    }

    [Fact]
    public async Task CreateTask_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var task = new TaskModel { Id = 42, Name = "UnitTest Task" };
        var context = new DefaultHttpContext();
        var request = context.Request;

        _mockService.Setup(s => s.CreateTaskAsync(task, It.IsAny<IDictionary<string, string>>()))
                    .Returns(Task.CompletedTask);

        // Act
        var result = await AccessorEndpoints.CreateTaskAsync(
            task,
            request,
            _mockService.Object,
            _mockLogger.Object,
            CancellationToken.None);

        // Assert
        var okResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var value = okResult.Value;

        Assert.NotNull(value);
        Assert.Equal("Task 42 Saved", value?.ToString());
    }

    [Fact]
    public async Task UpdateTaskName_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var requestModel = new UpdateTaskName { Id = 99, Name = "Updated Name" };
        var context = new DefaultHttpContext();
        var httpRequest = context.Request;

        _mockService.Setup(s => s.UpdateTaskNameAsync(99, "Updated Name", It.IsAny<IDictionary<string, string>>()))
                    .ReturnsAsync(true);

        // Act
        var result = await AccessorEndpoints.UpdateTaskNameAsync(
            requestModel,
            httpRequest,
            _mockService.Object,
            _mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal($"Task {requestModel.Id} updated successfully.", okResult.Value);
    }

    [Fact]
    public async Task UpdateTaskName_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var requestModel = new UpdateTaskName { Id = 999, Name = "DoesNotExist" };
        var context = new DefaultHttpContext();
        var httpRequest = context.Request;

        _mockService.Setup(s => s.UpdateTaskNameAsync(requestModel.Id, requestModel.Name, It.IsAny<IDictionary<string, string>>()))
                    .ReturnsAsync(false);

        // Act
        var result = await AccessorEndpoints.UpdateTaskNameAsync(
            requestModel,
            httpRequest,
            _mockService.Object,
            _mockLogger.Object);

        // Assert
        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, statusResult.StatusCode);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var value = valueResult.Value;
        Assert.Equal($"Task with ID {requestModel.Id} not found.", value?.ToString());
    }

    [Fact]
    public async Task DeleteTask_ReturnsOk_WhenDeleted()
    {
        // Arrange
        var taskId = 15;
        var context = new DefaultHttpContext();
        var request = context.Request;

        _mockService.Setup(s => s.DeleteTaskAsync(taskId, It.IsAny<IDictionary<string, string>>()))
                    .ReturnsAsync(true);

        // Act
        var result = await AccessorEndpoints.DeleteTaskAsync(
            taskId,
            request,
            _mockService.Object,
            _mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal($"Task {taskId} deleted.", okResult.Value);
    }

    [Fact]
    public async Task DeleteTask_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = 123;
        var context = new DefaultHttpContext();
        var request = context.Request;

        _mockService.Setup(s => s.DeleteTaskAsync(taskId, It.IsAny<IDictionary<string, string>>()))
                    .ReturnsAsync(false);

        // Act
        var result = await AccessorEndpoints.DeleteTaskAsync(
            taskId,
            request,
            _mockService.Object,
            _mockLogger.Object);

        // Assert
        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, statusResult.StatusCode);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var value = valueResult.Value;
        Assert.Equal($"Task with ID {taskId} not found.", value?.ToString());
    }
}
