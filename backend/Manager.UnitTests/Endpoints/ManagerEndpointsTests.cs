using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Manager.Models;
using Manager.UnitTests.TestHelpers;
using Moq;
using Xunit;

namespace Manager.UnitTests.Endpoints;

public class ManagerEndpointsTests : IClassFixture<CustomWebAppFactory>
{
    private readonly CustomWebAppFactory _factory;

    public ManagerEndpointsTests(CustomWebAppFactory factory) => _factory = factory;

    // -------- GET /task/{id} --------

    [Fact]
    public async Task GetTask_Found_Returns200_WithBody()
    {
        var task = new TaskModel { Id = 1, Name = "n", Payload = "p" };
        _factory.Mocks.ManagerService.Setup(m => m.GetTaskAsync(1)).ReturnsAsync(task);

        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/task/1");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await resp.Content.ReadFromJsonAsync<TaskModel>())!.Should().BeEquivalentTo(task);
    }

    [Fact]
    public async Task GetTask_NotFound_Returns404()
    {
        _factory.Mocks.ManagerService.Setup(m => m.GetTaskAsync(77)).ReturnsAsync((TaskModel?)null);

        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/task/77");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetTask_WhenServiceThrows_ReturnsProblem500()
    {
        _factory.Mocks.ManagerService.Setup(m => m.GetTaskAsync(5)).ThrowsAsync(new Exception("boom"));

        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/task/5");

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // -------- POST /task --------

    [Fact]
    public async Task CreateTask_InvalidModel_Returns400()
    {
        var client = _factory.CreateClient();
        var invalid = new { id = 2, name = "", payload = "" }; // triggers ValidationExtensions failure

        var resp = await client.PostAsJsonAsync("/task", invalid);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_Valid_Returns202_WithLocation()
    {
        _factory.Mocks.ManagerService
            .Setup(m => m.ProcessTaskAsync(It.IsAny<TaskModel>()))
            .ReturnsAsync((true, "ok"));

        var client = _factory.CreateClient();
        var req = new TaskModel { Id = 9, Name = "good", Payload = "x" };

        var resp = await client.PostAsJsonAsync("/task", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Accepted);
        resp.Headers.Location!.ToString().Should().Be("/task/9");
    }

    [Fact]
    public async Task CreateTask_WhenServiceReturnsFalse_ReturnsProblem500()
    {
        _factory.Mocks.ManagerService
            .Setup(m => m.ProcessTaskAsync(It.IsAny<TaskModel>()))
            .ReturnsAsync((false, "bad"));

        var client = _factory.CreateClient();
        var req = new TaskModel { Id = 10, Name = "n", Payload = "p" };

        var resp = await client.PostAsJsonAsync("/task", req);

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateTask_WhenServiceThrows_ReturnsProblem500()
    {
        _factory.Mocks.ManagerService
            .Setup(m => m.ProcessTaskAsync(It.IsAny<TaskModel>()))
            .ThrowsAsync(new Exception("boom"));

        var client = _factory.CreateClient();
        var req = new TaskModel { Id = 11, Name = "n", Payload = "p" };

        var resp = await client.PostAsJsonAsync("/task", req);

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // -------- POST /tasklong --------

    [Fact]
    public async Task CreateTaskLong_Accepts_Returns202()
    {
        _factory.Mocks.ManagerService
            .Setup(m => m.ProcessTaskLongAsync(It.IsAny<TaskModel>()))
            .ReturnsAsync((true, "ok"));

        var client = _factory.CreateClient();
        var req = new TaskModel { Id = 12, Name = "n", Payload = "p" };

        var resp = await client.PostAsJsonAsync("/tasklong", req);

        resp.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task CreateTaskLong_WhenServiceThrows_ReturnsProblem500()
    {
        _factory.Mocks.ManagerService
            .Setup(m => m.ProcessTaskLongAsync(It.IsAny<TaskModel>()))
            .ThrowsAsync(new Exception("x"));

        var client = _factory.CreateClient();
        var req = new TaskModel { Id = 12, Name = "n", Payload = "p" };

        var resp = await client.PostAsJsonAsync("/tasklong", req);

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // -------- PUT /task/{id}/{name} --------

    [Fact]
    public async Task UpdateTaskName_Success_Returns200()
    {
        _factory.Mocks.ManagerService.Setup(m => m.UpdateTaskName(5, "new")).ReturnsAsync(true);

        var client = _factory.CreateClient();
        var resp = await client.PutAsync("/task/5/new", null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateTaskName_NotFound_Returns404()
    {
        _factory.Mocks.ManagerService.Setup(m => m.UpdateTaskName(77, "x")).ReturnsAsync(false);

        var client = _factory.CreateClient();
        var resp = await client.PutAsync("/task/77/x", null);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTaskName_WhenServiceThrows_ReturnsProblem500()
    {
        _factory.Mocks.ManagerService.Setup(m => m.UpdateTaskName(6, "x")).ThrowsAsync(new Exception("err"));

        var client = _factory.CreateClient();
        var resp = await client.PutAsync("/task/6/x", null);

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // -------- DELETE /task/{id} --------

    [Fact]
    public async Task DeleteTask_Success_Returns200()
    {
        _factory.Mocks.ManagerService.Setup(m => m.DeleteTask(13)).ReturnsAsync(true);

        var client = _factory.CreateClient();
        var resp = await client.DeleteAsync("/task/13");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteTask_NotFound_Returns404()
    {
        _factory.Mocks.ManagerService.Setup(m => m.DeleteTask(14)).ReturnsAsync(false);

        var client = _factory.CreateClient();
        var resp = await client.DeleteAsync("/task/14");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_WhenServiceThrows_ReturnsProblem500()
    {
        _factory.Mocks.ManagerService.Setup(m => m.DeleteTask(15)).ThrowsAsync(new Exception("oops"));

        var client = _factory.CreateClient();
        var resp = await client.DeleteAsync("/task/15");

        resp.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
