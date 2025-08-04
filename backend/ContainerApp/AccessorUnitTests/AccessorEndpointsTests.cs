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

        _mockService.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(task);

        // Act
        var result = await AccessorEndpoints.GetTaskByIdAsync(1, _mockService.Object, _mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<TaskModel>>(result);
        Assert.Equal(1, okResult.Value?.Id);
    }

    [Fact]
    public async Task GetTaskById_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var taskId = 999; // non-existing task ID
        _mockService.Setup(s => s.GetTaskByIdAsync(taskId)).ReturnsAsync((TaskModel?)null);

        // Act
        var result = await AccessorEndpoints.GetTaskByIdAsync(taskId, _mockService.Object, _mockLogger.Object);

        // Assert
        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, statusResult.StatusCode);
        var notFoundResult = Assert.IsType<IValueHttpResult>(result, exactMatch: false);
        var response = notFoundResult.Value;

        Assert.NotNull(response);
        var messageProp = response.GetType().GetProperty("Message")?.GetValue(response)?.ToString();
        Assert.Equal($"Task with ID {taskId} not found", messageProp);
    }

    [Fact]
    public async Task CreateTask_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var task = new TaskModel { Id = 42, Name = "UnitTest Task" };

        // Act
        var result = await AccessorEndpoints.CreateTaskAsync(task, _mockService.Object, _mockLogger.Object);
        // Assert
        var okResult = Assert.IsType<IValueHttpResult>(result, exactMatch: false);

        var value = okResult.Value;

        Assert.NotNull(value);

        // Use reflection to inspect anonymous object properties
        var idProp = value.GetType().GetProperty("Id")?.GetValue(value);
        var statusProp = value.GetType().GetProperty("Status")?.GetValue(value);

        Assert.Equal(42, idProp);
        Assert.Equal("Saved", statusProp);
    }

    [Fact]
    public async Task UpdateTaskName_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var request = new UpdateTaskName { Id = 99, Name = "Updated Name" };

        _mockService.Setup(s => s.UpdateTaskNameAsync(99, "Updated Name")).ReturnsAsync(true);

        // Act
        var result = await AccessorEndpoints.UpdateTaskNameAsync(request, _mockService.Object, _mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal($"Task {request.Id} updated successfully.", okResult.Value);
    }

    [Fact]
    public async Task UpdateTaskName_ReturnsNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var request = new UpdateTaskName { Id = 999, Name = "DoesNotExist" };

        // Simulate task not found
        _mockService.Setup(s => s.UpdateTaskNameAsync(request.Id, request.Name)).ReturnsAsync(false);

        // Act
        var result = await AccessorEndpoints.UpdateTaskNameAsync(request, _mockService.Object, _mockLogger.Object);

        // Assert
        var statusResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(404, statusResult.StatusCode);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        var value = valueResult.Value;
        Assert.Equal($"Task with ID {request.Id} not found.", value?.ToString());
    }

    [Fact]
    public async Task DeleteTask_ReturnsOk_WhenDeleted()
    {
        // Arrange
        var taskId = 15;

        _mockService.Setup(s => s.DeleteTaskAsync(taskId)).ReturnsAsync(true);

        // Act
        var result = await AccessorEndpoints.DeleteTaskAsync(taskId, _mockService.Object, _mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal($"Task {taskId} deleted.", okResult.Value);
    }
}
