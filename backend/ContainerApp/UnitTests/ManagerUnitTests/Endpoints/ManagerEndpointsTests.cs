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
    }

    [Fact(DisplayName = "POST /task => 202 + body when valid")]
    public async Task CreateTask_Returns_Accepted_When_Valid()
    {
        var model = MakeTask(42, "UnitTest Task");

        _svc.Setup(s => s.ProcessTaskAsync(model))
            .ReturnsAsync((true, "queued"));

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "CreateTaskAsync",
            model,
            _svc.Object);

        // 1) Assert status via interface (Accepted<T> is anonymous generic)
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
    }

    [Fact(DisplayName = "PUT /task/{id}/{name} => 200 on success, 404 otherwise")]
    public async Task UpdateTaskName_Behaves_As_Expected()
    {
        _svc.Setup(s => s.UpdateTaskName(10, "new-name")).ReturnsAsync(true);
        _svc.Setup(s => s.UpdateTaskName(11, "missing")).ReturnsAsync(false);

        var okRes = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "UpdateTaskNameAsync",
            10, "new-name",
            _svc.Object);
        Assert.IsType<Ok<string>>(okRes);

        var nfRes = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "UpdateTaskNameAsync",
            11, "missing",
            _svc.Object);
        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(nfRes);
        Assert.Equal(StatusCodes.Status404NotFound, status.StatusCode);
    }

    [Fact(DisplayName = "DELETE /task/{id} => 200 when deleted, 404 when missing")]
    public async Task DeleteTask_Behaves_As_Expected()
    {
        _svc.Setup(s => s.DeleteTask(5)).ReturnsAsync(true);
        _svc.Setup(s => s.DeleteTask(6)).ReturnsAsync(false);

        var ok = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "DeleteTaskAsync",
            5,
            _svc.Object);
        Assert.IsType<Ok<string>>(ok);

        var nf = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(ManagerEndpoints),
            "DeleteTaskAsync",
            6,
            _svc.Object);
        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(nf);
        Assert.Equal(StatusCodes.Status404NotFound, status.StatusCode);
    }
}
