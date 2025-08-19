using Manager.Endpoints;
using Manager.Models;
using Manager.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Moq;
using System.Text.Json;

namespace ManagerUnitTests.Endpoints;
public class ManagerEndpointsTests
{
    private readonly Mock<IManagerService> _svc = new(MockBehavior.Strict);

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
                                        typeof(ManagerEndpoints),
                                        "UpdateTaskNameAsync",
                                        id, name!, _svc.Object),
            "DeleteTaskAsync" => PrivateInvoker.InvokePrivateEndpointAsync(
                                        typeof(ManagerEndpoints),
                                        "DeleteTaskAsync",
                                        id, _svc.Object),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown endpoint")
        };

    [Fact(DisplayName = "GET /task/{id} => 200 when found")]
    public async Task GetTask_Returns_Ok_When_Found()
    {
        var task = MakeTask(7, "X");
        _svc.Setup(s => s.GetTaskAsync(7)).ReturnsAsync(task);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "GetTaskAsync",
            7,
            _svc.Object);

        var ok = Assert.IsType<Ok<TaskModel>>(result);
        Assert.Equal(7, ok.Value!.Id);
        Assert.Equal("X", ok.Value!.Name);
        _svc.VerifyAll();
    }

    [Fact(DisplayName = "GET /task/{id} => 404 when missing")]
    public async Task GetTask_Returns_NotFound_When_Missing()
    {
        _svc.Setup(s => s.GetTaskAsync(999)).ReturnsAsync((TaskModel?)null);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "GetTaskAsync",
            999,
            _svc.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, status.StatusCode);
        _svc.VerifyAll();
    }

    [Fact(DisplayName = "POST /task => 202 + body when valid")]
    public async Task CreateTask_Returns_Accepted_When_Valid()
    {
        var model = MakeTask(42, "UnitTest Task");

        _svc.Setup(s => s.CreateTaskAsync(model))
            .ReturnsAsync((true, "queued"));

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "CreateTaskAsync",
            model,
            _svc.Object);

        // 1) Assert status via interface (Accepted<T> is generic)
        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, status.StatusCode);

        // 2) Location (property exists on Accepted<T>)
        var location = result.GetType().GetProperty("Location")?.GetValue(result) as string;
        Assert.Equal("/task/42", location);

        // 3) Assert payload via IValueHttpResult (anonymous object)
        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(valueResult.Value));
        var root = doc.RootElement;
        Assert.Equal("queued", root.GetProperty("status").GetString());
        Assert.Equal(42, root.GetProperty("Id").GetInt32());

        _svc.VerifyAll();
    }

    [Fact(DisplayName = "POST /tasklong => 202 Accepted")]
    public async Task CreateTaskLong_Returns_Accepted()
    {
        var model = MakeTask(77, "Long");

        _svc.Setup(s => s.ProcessTaskLongAsync(It.IsAny<TaskModel>()))
            .ReturnsAsync((true, "accepted"));

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "CreateTaskLongAsync",
            model,
            _svc.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, status.StatusCode);
        _svc.VerifyAll();
    }

    [Theory(DisplayName = "Update/Delete => 200 when true, 404 when false")]
    [InlineData("UpdateTaskNameAsync", true, typeof(Ok<string>), StatusCodes.Status404NotFound)]
    [InlineData("DeleteTaskAsync", true, typeof(Ok<string>), StatusCodes.Status404NotFound)]
    public async Task Endpoint_Returns_Correct_Status(
        string method, bool successFirst, Type okType, int notFoundStatus)
    {
        var seq = method switch
        {
            "UpdateTaskNameAsync" => _svc.SetupSequence(s => s.UpdateTaskName(It.IsAny<int>(), It.IsAny<string>())),
            "DeleteTaskAsync" => _svc.SetupSequence(s => s.DeleteTask(It.IsAny<int>())),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown endpoint")
        };

        seq.ReturnsAsync(successFirst)
           .ReturnsAsync(false);

        var okRes = await Invoke(method, 10, "new-name");
        Assert.IsType(okType, okRes);

        var nfRes = await Invoke(method, 11, "missing");
        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(nfRes);
        Assert.Equal(notFoundStatus, status.StatusCode);

        _svc.VerifyAll();
    }
}
