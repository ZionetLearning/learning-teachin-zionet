using Accessor.Endpoints;

namespace AccessorUnitTests;

using Xunit;
using Moq;
using Accessor.Models;
using Accessor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.HttpResults;


public class AccessorEndpointsTests
{
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
}
