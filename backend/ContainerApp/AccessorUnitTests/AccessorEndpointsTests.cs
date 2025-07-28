using Accessor.Endpoints;
using Microsoft.AspNetCore.Http;
using Xunit.Abstractions;

namespace AccessorUnitTests;

using Xunit;
using Moq;
using Accessor.Models;
using Accessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;


public class AccessorEndpointsTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AccessorEndpointsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GetTaskById_ReturnsOk_WhenTaskExists()
    {
        // Arrange
        var mockService = new Mock<IAccessorService>();
        var mockLogger = new Mock<ILogger<AccessorService>>();
        var task = new TaskModel { Id = 1, Name = "Test" };

        mockService.Setup(s => s.GetTaskByIdAsync(1)).ReturnsAsync(task);

        // Act
        var result = await AccessorEndpoints.GetTaskById(1, mockService.Object, mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<TaskModel>>(result);
        Assert.Equal(1, okResult.Value?.Id);
    }
    
    [Fact]
    public async Task SaveTask_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var mockService = new Mock<IAccessorService>();
        var mockLogger = new Mock<ILogger<AccessorService>>();
        var task = new TaskModel { Id = 42, Name = "UnitTest Task" };

        // Act
        var result = await AccessorEndpoints.SaveTask(task, mockService.Object, mockLogger.Object);
        // Assert
        var okResult = Assert.IsType<IValueHttpResult>(result, exactMatch: false);

        var value = okResult.Value;

        Assert.NotNull(value);

        // Use reflection to inspect anonymous object properties
        var idProp = value.GetType().GetProperty("Id")?.GetValue(value);
        var statusProp = value.GetType().GetProperty("Status")?.GetValue(value);

        _testOutputHelper.WriteLine("Id: " + idProp);
        _testOutputHelper.WriteLine("Status: " + statusProp);

        Assert.Equal(42, idProp);
        Assert.Equal("Saved", statusProp);
    }
    
    [Fact]
    public async Task UpdateTaskName_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var mockService = new Mock<IAccessorService>();
        var mockLogger = new Mock<ILogger<AccessorService>>();
        var request = new UpdateTaskName { Id = 99, Name = "Updated Name" };

        mockService.Setup(s => s.UpdateTaskNameAsync(99, "Updated Name")).ReturnsAsync(true);

        // Act
        var result = await AccessorEndpoints.UpdateTaskName(request, mockService.Object, mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Contains("updated successfully", okResult.Value);
        Assert.Contains(request.Id.ToString(), okResult.Value);
    }
    
    [Fact]
    public async Task DeleteTask_ReturnsOk_WhenDeleted()
    {
        // Arrange
        var mockService = new Mock<IAccessorService>();
        var mockLogger = new Mock<ILogger<AccessorService>>();
        int taskId = 15;

        mockService.Setup(s => s.DeleteTaskAsync(taskId)).ReturnsAsync(true);

        // Act
        var result = await AccessorEndpoints.DeleteTask(taskId, mockService.Object, mockLogger.Object);

        // Assert
        var okResult = Assert.IsType<Ok<string>>(result);
        Assert.Equal("Task 15 deleted.", okResult.Value);
    }



}
