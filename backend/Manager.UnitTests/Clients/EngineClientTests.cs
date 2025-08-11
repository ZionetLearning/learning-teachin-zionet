using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using FluentAssertions;
using Manager.Constants;
using Manager.Models;
using Manager.Services.Clients;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Manager.UnitTests.Clients;

public class EngineClientTests
{
    [Fact]
    public async Task ProcessTaskAsync_SendsBinding_And_ReturnsSuccess()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var log = new Mock<ILogger<EngineClient>>();
        var sut = new EngineClient(log.Object, dapr.Object);

        // match the overload: InvokeBindingAsync(string, string, object, Dictionary<string,string>?, CancellationToken)
        dapr.Setup(d => d.InvokeBindingAsync(
                QueueNames.ManagerToEngine, "create", It.IsAny<object>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var (ok, msg) = await sut.ProcessTaskAsync(new TaskModel { Id = 1, Name = "n", Payload = "p" });

        ok.Should().BeTrue();
        msg.Should().Be("sent to engine");
        dapr.VerifyAll();
    }

    [Fact]
    public async Task ProcessTaskAsync_WhenDaprThrows_Rethrows()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new EngineClient(new Mock<ILogger<EngineClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeBindingAsync(
                QueueNames.ManagerToEngine, "create", It.IsAny<object>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("boom"));

        await FluentActions.Invoking(() =>
                sut.ProcessTaskAsync(new TaskModel { Id = 10, Name = "n", Payload = "p" }))
            .Should().ThrowAsync<System.Exception>();
    }

    [Fact]
    public async Task ProcessTaskLongAsync_SendsBinding_And_ReturnsSuccess()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new EngineClient(new Mock<ILogger<EngineClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeBindingAsync(
                QueueNames.ManagerToEngine, "create", It.IsAny<object>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var (ok, msg) = await sut.ProcessTaskLongAsync(new TaskModel { Id = 2, Name = "n", Payload = "p" });

        ok.Should().BeTrue();
        msg.Should().Be("sent to engine");
        dapr.VerifyAll();
    }

    [Fact]
    public async Task ProcessTaskLongAsync_WhenDaprThrows_Rethrows()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new EngineClient(new Mock<ILogger<EngineClient>>().Object, dapr.Object);

        dapr.Setup(d => d.InvokeBindingAsync(
                QueueNames.ManagerToEngine, "create", It.IsAny<object>(),
                It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("fail"));

        await FluentActions.Invoking(() =>
                sut.ProcessTaskLongAsync(new TaskModel { Id = 11, Name = "n", Payload = "p" }))
            .Should().ThrowAsync<System.Exception>();
    }

    [Fact]
    public async Task ChatAsync_InvokesMethod_And_ReturnsResponse()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new EngineClient(new Mock<ILogger<EngineClient>>().Object, dapr.Object);

        // Your DTOs require constructor args:
        // ChatRequestDto(string threadId, string userMessage, string ???)
        var req = new ChatRequestDto("t", "hi", "");
        var res = new ChatResponseDto("t", "ok");

        // match the overload with explicit CancellationToken
        dapr.Setup(d => d.InvokeMethodAsync<ChatRequestDto, ChatResponseDto>(
                AppIds.Engine, "chat", It.IsAny<ChatRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(res);

        var got = await sut.ChatAsync(req, CancellationToken.None);

        got.Should().BeEquivalentTo(res);
        dapr.VerifyAll();
    }

    [Fact]
    public async Task ChatAsync_WhenDaprThrows_Rethrows()
    {
        var dapr = new Mock<DaprClient>(MockBehavior.Strict);
        var sut = new EngineClient(new Mock<ILogger<EngineClient>>().Object, dapr.Object);

        var req = new ChatRequestDto("t", "hi", "");

        dapr.Setup(d => d.InvokeMethodAsync<ChatRequestDto, ChatResponseDto>(
                AppIds.Engine, "chat", It.IsAny<ChatRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new System.Exception("unavailable"));

        await FluentActions.Invoking(() => sut.ChatAsync(req, CancellationToken.None))
            .Should().ThrowAsync<System.Exception>();
    }
}