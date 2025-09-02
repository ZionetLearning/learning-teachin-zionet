using Manager.Endpoints;
using Manager.Models;
using Manager.Services;
using Manager.Services.Clients.Accessor;
using Manager.Services.Clients.Engine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace ManagerUnitTests.Endpoints;
public class ManagerEndpointsTests
{
    private readonly Mock<IAccessorClient> _accessor = new(MockBehavior.Strict);
    private readonly Mock<IEngineClient> _engine = new(MockBehavior.Strict);
    private readonly Mock<ILogger> _log = new();

    private static TaskModel MakeTask(int id, string name) => new()
    {
        Id = id,
        Name = name,
        Payload = "{}"
    };
    private Task<IResult> Invoke(string method, int id, string? name = null) =>
        method switch
        {
            "UpdateTaskNameAsync" => PrivateInvoker.InvokePrivateEndpointAsync(
                                        typeof(TasksEndpoints),
                                        "UpdateTaskNameAsync",
                                        id, name!, _accessor.Object, _log.Object),
            "DeleteTaskAsync" => PrivateInvoker.InvokePrivateEndpointAsync(
                                        typeof(TasksEndpoints),
                                        "DeleteTaskAsync",
                                        id, _accessor.Object, _log.Object),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown endpoint")
        };

   [Fact(DisplayName = "GET /task/{id} => 200 when found")]
    public async Task GetTask_Returns_Ok_When_Found()
    {
        var task = MakeTask(7, "X");
        _accessor.Setup(s => s.GetTaskAsync(7)).ReturnsAsync(task);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(TasksEndpoints),
            "GetTaskAsync",
            7,
            _accessor.Object,
            _log.Object);

        var ok = Assert.IsType<Ok<TaskModel>>(result);
        Assert.Equal(7, ok.Value!.Id);
        Assert.Equal("X", ok.Value!.Name);

        _accessor.VerifyAll();
    }

    [Fact(DisplayName = "GET /task/{id} => 404 when missing")]
    public async Task GetTask_Returns_NotFound_When_Missing()
    {
        _accessor.Setup(s => s.GetTaskAsync(999)).ReturnsAsync((TaskModel?)null);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(TasksEndpoints),
            "GetTaskAsync",
            999,
            _accessor.Object,
            _log.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, status.StatusCode);

        _accessor.VerifyAll();
    }

    [Fact(DisplayName = "POST /task => 202 + body when valid")]
    public async Task CreateTask_Returns_Accepted_When_Valid()
    {
        var model = MakeTask(42, "UnitTest Task");

        _accessor.Setup(s => s.PostTaskAsync(model))
                 .ReturnsAsync((true, "queued"));

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(TasksEndpoints),
            "CreateTaskAsync",
            model,
            _accessor.Object,
            _log.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, status.StatusCode);

        var location = result.GetType().GetProperty("Location")?.GetValue(result) as string;
        Assert.Equal("/tasks-manager/task/42", location);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(valueResult.Value));
        var root = doc.RootElement;
        Assert.Equal("queued", root.GetProperty("status").GetString());
        Assert.Equal(42, root.GetProperty("Id").GetInt32());

        _accessor.VerifyAll();
    }

    [Fact(DisplayName = "POST /tasklong => 202 Accepted")]
    public async Task CreateTaskLong_Returns_Accepted()
    {
        var model = MakeTask(77, "Long");

        _engine.Setup(s => s.ProcessTaskLongAsync(It.IsAny<TaskModel>()))
               .ReturnsAsync((true, "accepted"));

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(TasksEndpoints),
            "CreateTaskLongAsync",
            model,
            _engine.Object,
            _log.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, status.StatusCode);

        _engine.VerifyAll();
    }

    [Theory(DisplayName = "Update/Delete => 200 when true, 404 when false")]
    [InlineData("UpdateTaskNameAsync", true, typeof(Ok<string>), StatusCodes.Status404NotFound)]
    [InlineData("DeleteTaskAsync", true, typeof(Ok<string>), StatusCodes.Status404NotFound)]
    public async Task Endpoint_Returns_Correct_Status(
        string method, bool successFirst, Type okType, int notFoundStatus)
    {
        // Arrange: setup sequence based on method
        var seq = method switch
        {
            "UpdateTaskNameAsync" => _accessor.SetupSequence(s => s.UpdateTaskName(It.IsAny<int>(), It.IsAny<string>())),
            "DeleteTaskAsync"     => _accessor.SetupSequence(s => s.DeleteTask(It.IsAny<int>())),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown endpoint")
        };

        seq.ReturnsAsync(successFirst).ReturnsAsync(false);

        // Act + Assert: common helper for both methods
        await AssertEndpointBehavior(method, okType, notFoundStatus);

        _accessor.VerifyAll();
    }

    private async Task AssertEndpointBehavior(string method, Type okType, int notFoundStatus)
    {
        // Successful case
        var okRes = method switch
        {
            "UpdateTaskNameAsync" => await Invoke(method, 10, "new-name"),
            "DeleteTaskAsync"     => await Invoke(method, 10),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method)
        };
        Assert.IsType(okType, okRes);

        // Not found case
        var nfRes = method switch
        {
            "UpdateTaskNameAsync" => await Invoke(method, 11, "missing"),
            "DeleteTaskAsync"     => await Invoke(method, 11),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method)
        };
        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(nfRes);
        Assert.Equal(notFoundStatus, status.StatusCode);
    }
}
