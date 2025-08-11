using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using FluentAssertions;
using Manager.Constants;
using Manager.Models;
using Manager.Services.Clients;
using Manager.UnitTests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Manager.UnitTests.Clients;

public class AccessorClientTests
{
    [Fact]
    public async Task GetTaskAsync_ReturnsTask()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var log = new Mock<ILogger<AccessorClient>>();
        var sut = new AccessorClient(log.Object, dapr.Object);

        var t = new TaskModel { Id = 2, Name = "n", Payload = "p" };
        dapr.Setup(d => d.InvokeMethodAsync<TaskModel?>(
                HttpMethod.Get, "accessor", "task/2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(t);

        var result = await sut.GetTaskAsync(2);

        result.Should().BeEquivalentTo(t);
        dapr.VerifyAll();
    }

    [Fact]
    public async Task GetTaskAsync_404_ReturnsNull()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new AccessorClient(new Mock<ILogger<AccessorClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeMethodAsync<TaskModel?>(
                HttpMethod.Get, "accessor", "task/1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(DaprTestHelpers.NotFoundInvocation());

        var result = await sut.GetTaskAsync(1);

        result.Should().BeNull();
        dapr.VerifyAll();
    }

    [Fact]
    public async Task UpdateTaskName_TaskMissing_ReturnsFalse()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new AccessorClient(new Mock<ILogger<AccessorClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeMethodAsync<TaskModel?>(
                HttpMethod.Get, "accessor", "task/5", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskModel?)null);

        var ok = await sut.UpdateTaskName(5, "new");

        ok.Should().BeFalse();
        dapr.VerifyAll();
    }

    [Fact]
    public async Task UpdateTaskName_WhenFound_SendsBinding_And_ReturnsTrue()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new AccessorClient(new Mock<ILogger<AccessorClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeMethodAsync<TaskModel?>(
                HttpMethod.Get, "accessor", "task/5", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskModel { Id = 5, Name = "n", Payload = "p" });

        dapr.Setup(d => d.InvokeBindingAsync(
                QueueNames.TaskUpdate, "create", It.IsAny<object>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ok = await sut.UpdateTaskName(5, "new");

        ok.Should().BeTrue();
        dapr.VerifyAll();
    }

    [Fact]
    public async Task DeleteTask_404_ReturnsFalse()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new AccessorClient(new Mock<ILogger<AccessorClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeMethodAsync(
                HttpMethod.Delete, "accessor", "task/9", It.IsAny<CancellationToken>()))
            .ThrowsAsync(DaprTestHelpers.NotFoundInvocation());

        var ok = await sut.DeleteTask(9);

        ok.Should().BeFalse();
        dapr.VerifyAll();
    }

    [Fact]
    public async Task DeleteTask_Success_ReturnsTrue()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new AccessorClient(new Mock<ILogger<AccessorClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeMethodAsync(
                HttpMethod.Delete, "accessor", "task/3", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var ok = await sut.DeleteTask(3);

        ok.Should().BeTrue();
        dapr.VerifyAll();
    }
}