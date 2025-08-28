using AutoMapper;
using FluentAssertions;
using Manager.Models;
using Manager.Services;
using Manager.Services.Clients;
using Manager.Services.Clients.Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Manager.UnitTests.Services;

public class ManagerServiceTests
{
    private readonly Mock<IAccessorClient> _accessor = new();
    private readonly Mock<IEngineClient> _engine = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly IConfiguration _cfg = new ConfigurationBuilder().Build();
    private readonly Mock<ILogger<ManagerService>> _log = new();
    private readonly Mock<INotificationService> _notification = new();

    private ManagerService Create() =>
        new(_cfg, _log.Object, _accessor.Object, _engine.Object, _mapper.Object, _notification.Object);
    // ---------- GetTaskAsync ----------

    [Fact]
    public async Task GetTaskAsync_IdLessOrEqualZero_ReturnsNull_AndDoesNotCallAccessor()
    {
        var sut = Create();

        var res = await sut.GetTaskAsync(0);

        res.Should().BeNull();
        _accessor.Verify(a => a.GetTaskAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetTaskAsync_WhenFound_ReturnsTask()
    {
        var sut = Create();
        var t = new TaskModel { Id = 7, Name = "n", Payload = "p" };
        _accessor.Setup(a => a.GetTaskAsync(7)).ReturnsAsync(t);

        var res = await sut.GetTaskAsync(7);

        res.Should().Be(t);
        _accessor.VerifyAll();
    }

    [Fact]
    public async Task GetTaskAsync_WhenAccessorThrows_Rethrows()
    {
        var sut = Create();
        _accessor.Setup(a => a.GetTaskAsync(1)).ThrowsAsync(new InvalidOperationException("boom"));

        await FluentActions.Invoking(() => sut.GetTaskAsync(1))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    // ---------- CreateTaskAsync ----------

    [Fact]
    public async Task CreateTaskAsync_NullTask_ReturnsFalse_WithMessage()
    {
        var sut = Create();

        var (ok, msg) = await sut.CreateTaskAsync(null!);

        ok.Should().BeFalse();
        msg.Should().Be("Task is null");
    }

    [Theory]
    [InlineData("", "p", "Task name is required")]
    [InlineData("n", "", "Task payload is required")]
    [InlineData("   ", "p", "Task name is required")]
    [InlineData("n", "   ", "Task payload is required")]
    public async Task CreateTaskAsync_InvalidFields_ReturnsFalse_WithMessage(string name, string payload, string expectedMsg)
    {
        var sut = Create();
        var task = new TaskModel { Id = 5, Name = name, Payload = payload };

        var (ok, msg) = await sut.CreateTaskAsync(task);

        ok.Should().BeFalse();
        msg.Should().Be(expectedMsg);
        _accessor.Verify(a => a.PostTaskAsync(It.IsAny<TaskModel>(), null), Times.Never);
    }

    [Fact]
    public async Task CreateTaskAsync_AccessorSuccess_PassesThroughResult()
    {
        var sut = Create();
        var t = new TaskModel { Id = 2, Name = "ok", Payload = "p" };
        _accessor.Setup(a => a.PostTaskAsync(t, null)).ReturnsAsync((true, "sent"));

        var (ok, msg) = await sut.CreateTaskAsync(t);

        ok.Should().BeTrue();
        msg.Should().Be("sent");
        _accessor.VerifyAll();
    }

    [Fact]
    public async Task CreateTaskAsync_AccessorFailure_PassesThroughFalseAndMessage()
    {
        var sut = Create();
        var t = new TaskModel { Id = 3, Name = "ok", Payload = "p" };
        _accessor.Setup(a => a.PostTaskAsync(t, null)).ReturnsAsync((false, "bad"));

        var (ok, msg) = await sut.CreateTaskAsync(t);

        ok.Should().BeFalse();
        msg.Should().Be("bad");
        _accessor.VerifyAll();
    }

    [Fact]
    public async Task CreateTaskAsync_AccessorThrows_ReturnsFriendlyFailure()
    {
        var sut = Create();
        var t = new TaskModel { Id = 4, Name = "ok", Payload = "p" };
        _accessor.Setup(a => a.PostTaskAsync(t, null)).ThrowsAsync(new Exception("boom"));

        var (ok, msg) = await sut.CreateTaskAsync(t);

        ok.Should().BeFalse();
        msg.Should().Be("Failed to send to Engine");
    }

    // ---------- ProcessTaskLongAsync ----------

    [Fact]
    public async Task ProcessTaskLongAsync_Success_PassesThrough()
    {
        var sut = Create();
        var t = new TaskModel { Id = 9, Name = "n", Payload = "p" };
        _engine.Setup(e => e.ProcessTaskLongAsync(t, null)).ReturnsAsync((true, "ok"));

        var (ok, msg) = await sut.ProcessTaskLongAsync(t);

        ok.Should().BeTrue();
        msg.Should().Be("ok");
        _engine.VerifyAll();
    }

    [Fact]
    public async Task ProcessTaskLongAsync_EngineThrows_ReturnsFriendlyFailure()
    {
        var sut = Create();
        var t = new TaskModel { Id = 9, Name = "n", Payload = "p" };
        _engine.Setup(e => e.ProcessTaskLongAsync(t, null)).ThrowsAsync(new Exception("x"));

        var (ok, msg) = await sut.ProcessTaskLongAsync(t);

        ok.Should().BeFalse();
        msg.Should().Be("Failed to send to Engine");
    }

    // ---------- UpdateTaskName ----------

    [Fact]
    public async Task UpdateTaskName_IdInvalidOrNameInvalid_ReturnsFalse_AndSkipsAccessor()
    {
        var sut = Create();

        (await sut.UpdateTaskName(0, "x")).Should().BeFalse();
        (await sut.UpdateTaskName(1, "")).Should().BeFalse();
        (await sut.UpdateTaskName(1, new string('a', 101))).Should().BeFalse();

        _accessor.Verify(a => a.UpdateTaskName(It.IsAny<int>(), It.IsAny<string>(), null), Times.Never);
    }

    [Fact]
    public async Task UpdateTaskName_Success_ReturnsTrue()
    {
        var sut = Create();
        _accessor.Setup(a => a.UpdateTaskName(5, "new", null)).ReturnsAsync(true);

        var ok = await sut.UpdateTaskName(5, "new");

        ok.Should().BeTrue();
        _accessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateTaskName_WhenAccessorReturnsFalse_ReturnsFalse()
    {
        var sut = Create();
        _accessor.Setup(a => a.UpdateTaskName(6, "x", null)).ReturnsAsync(false);

        var ok = await sut.UpdateTaskName(6, "x");

        ok.Should().BeFalse();
        _accessor.VerifyAll();
    }

    [Fact]
    public async Task UpdateTaskName_WhenAccessorThrows_ReturnsFalse()
    {
        var sut = Create();
        _accessor.Setup(a => a.UpdateTaskName(7, "x", null)).ThrowsAsync(new Exception("fail"));

        var ok = await sut.UpdateTaskName(7, "x");

        ok.Should().BeFalse();
    }

    // ---------- DeleteTask ----------

    [Fact]
    public async Task DeleteTask_IdInvalid_ReturnsFalse_AndSkipsAccessor()
    {
        var sut = Create();

        var ok = await sut.DeleteTask(0);

        ok.Should().BeFalse();
        _accessor.Verify(a => a.DeleteTask(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTask_WhenAccessorTrue_ReturnsTrue()
    {
        var sut = Create();
        _accessor.Setup(a => a.DeleteTask(8)).ReturnsAsync(true);

        var ok = await sut.DeleteTask(8);

        ok.Should().BeTrue();
        _accessor.VerifyAll();
    }

    [Fact]
    public async Task DeleteTask_WhenAccessorFalse_ReturnsFalse()
    {
        var sut = Create();
        _accessor.Setup(a => a.DeleteTask(8)).ReturnsAsync(false);

        var ok = await sut.DeleteTask(8);

        ok.Should().BeFalse();
        _accessor.VerifyAll();
    }

    [Fact]
    public async Task DeleteTask_WhenAccessorThrows_ReturnsFalse()
    {
        var sut = Create();
        _accessor.Setup(a => a.DeleteTask(8)).ThrowsAsync(new Exception("err"));

        var ok = await sut.DeleteTask(8);

        ok.Should().BeFalse();
    }
}