using Manager.Endpoints;
using Manager.Models;
using Manager.Models.Tasks.Requests;
using Manager.Models.Tasks.Responses;
using Manager.Services;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Tasks;
using Manager.Services.Clients.Engine;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace ManagerUnitTests.Endpoints;
public class ManagerEndpointsTests
{
    private readonly Mock<ITaskAccessorClient> _taskAccessor = new(MockBehavior.Strict);
    private readonly Mock<IEngineClient> _engine = new(MockBehavior.Strict);
    private readonly Mock<ILogger> _log = new();

    private static GetTaskAccessorResponse MakeGetTaskAccessorResponse(int id, string name) => new()
    {
        Id = id,
        Name = name,
        Payload = "{}"
    };

    private Task<IResult> Invoke(string method, int id, UpdateTaskNameRequest? request = null, string? ifMatch = null)
    {
        var http = new DefaultHttpContext();

        return method switch
        {
            "UpdateTaskNameAsync" => PrivateInvoker.InvokePrivateEndpointAsync(
                                        typeof(TasksEndpoints),
                                        "UpdateTaskNameAsync",
                                        id,
                                        request!,
                                        ifMatch,
                                        _taskAccessor.Object,
                                        _log.Object,
                                        http.Response),
            "DeleteTaskAsync" => PrivateInvoker.InvokePrivateEndpointAsync(
                                        typeof(TasksEndpoints),
                                        "DeleteTaskAsync",
                                        id,
                                        _taskAccessor.Object,
                                        _log.Object),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown endpoint")
        };
    }

    [Fact(DisplayName = "GET /task/{id} => 200 when found")]
    public async Task GetTask_Returns_Ok_When_Found()
    {
        var accessorResponse = MakeGetTaskAccessorResponse(7, "X");
        _taskAccessor.Setup(s => s.GetTaskWithEtagAsync(7, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((accessorResponse, "abc"));

        var http = new DefaultHttpContext();
        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(TasksEndpoints),
            "GetTaskAsync",
            7,
            _taskAccessor.Object,
            _log.Object,
            http.Response
        );

        var ok = Assert.IsType<Ok<GetTaskResponse>>(result);
        Assert.Equal(7, ok.Value!.Id);
        Assert.Equal("X", ok.Value!.Name);
        Assert.Equal("\"abc\"", http.Response.Headers.ETag.ToString());

        _taskAccessor.VerifyAll();
    }

    [Fact(DisplayName = "GET /task/{id} => 404 when missing")]
    public async Task GetTask_Returns_NotFound_When_Missing()
    {
        _taskAccessor.Setup(s => s.GetTaskWithEtagAsync(999, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(((GetTaskAccessorResponse? Task, string? ETag))(null, null));

        var http = new DefaultHttpContext();
        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(TasksEndpoints),
            "GetTaskAsync",
            999,
            _taskAccessor.Object,
            _log.Object,
            http.Response
        );

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, status.StatusCode);

        _taskAccessor.VerifyAll();
    }

    [Fact(DisplayName = "POST /task => 202 + body when valid")]
    public async Task CreateTask_Returns_Accepted_When_Valid()
    {
        var request = new CreateTaskRequest
        {
            Id = 42,
            Name = "UnitTest Task",
            Payload = "{}"
        };

        var accessorResponse = new CreateTaskAccessorResponse
        {
            Success = true,
            Message = "queued",
            Id = 42
        };

        _taskAccessor.Setup(s => s.PostTaskAsync(It.IsAny<CreateTaskAccessorRequest>()))
                 .ReturnsAsync(accessorResponse);

        var result = await PrivateInvoker.InvokePrivateEndpointAsync(
            typeof(TasksEndpoints),
            "CreateTaskAsync",
            request,
            _taskAccessor.Object,
            _log.Object);

        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(result);
        Assert.Equal(StatusCodes.Status202Accepted, status.StatusCode);

        var location = result.GetType().GetProperty("Location")?.GetValue(result) as string;
        Assert.Equal("/tasks-manager/task/42", location);

        var valueResult = Assert.IsAssignableFrom<IValueHttpResult>(result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(valueResult.Value));
        var root = doc.RootElement;
        Assert.Equal("queued", root.GetProperty("Status").GetString());
        Assert.Equal(42, root.GetProperty("Id").GetInt32());

        _taskAccessor.VerifyAll();
    }

    [Fact(DisplayName = "POST /tasklong => 202 Accepted")]
    public async Task CreateTaskLong_Returns_Accepted()
    {
        var model = new TaskModel
        {
            Id = 77,
            Name = "Long",
            Payload = "{}"
        };

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
    [InlineData("UpdateTaskNameAsync", true, typeof(Ok<UpdateTaskNameResponse>), StatusCodes.Status404NotFound)]
    [InlineData("DeleteTaskAsync", true, typeof(Ok<DeleteTaskResponse>), StatusCodes.Status404NotFound)]
    public async Task Endpoint_Returns_Correct_Status(
        string method, bool successFirst, Type okType, int notFoundStatus)
    {
        switch (method)
        {
            case "UpdateTaskNameAsync":
                _taskAccessor
                    .SetupSequence(s => s.UpdateTaskNameAsync(
                        It.IsAny<int>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UpdateTaskNameAccessorResponse
                    {
                        Updated = successFirst,
                        NotFound = !successFirst,
                        PreconditionFailed = false,
                        NewEtag = successFirst ? "etag-2" : null
                    })
                    .ReturnsAsync(new UpdateTaskNameAccessorResponse
                    {
                        Updated = false,
                        NotFound = true,
                        PreconditionFailed = false,
                        NewEtag = null
                    });
                break;

            case "DeleteTaskAsync":
                _taskAccessor
                    .SetupSequence(s => s.DeleteTask(It.IsAny<int>()))
                    .ReturnsAsync(successFirst)
                    .ReturnsAsync(false);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown endpoint");
        }

        // Act + Assert: common helper for both methods
        await AssertEndpointBehavior(method, okType, notFoundStatus);

        _taskAccessor.VerifyAll();
    }

    private async Task AssertEndpointBehavior(string method, Type okType, int notFoundStatus)
    {
        // Successful case
        var okRes = method switch
        {
            "UpdateTaskNameAsync" => await Invoke(method, 10, new UpdateTaskNameRequest { Name = "new-name" }, "\"etag-1\""),
            "DeleteTaskAsync" => await Invoke(method, 10),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method)
        };
        Assert.IsType(okType, okRes);

        // Not found case
        IResult nfRes = method switch
        {
            "UpdateTaskNameAsync" => await Invoke(method, 11, new UpdateTaskNameRequest { Name = "missing" }, "\"etag-1\""),
            "DeleteTaskAsync" => await Invoke(method, 11),
            _ => throw new ArgumentOutOfRangeException(nameof(method), method)
        };
        var status = Assert.IsAssignableFrom<IStatusCodeHttpResult>(nfRes);
        Assert.Equal(notFoundStatus, status.StatusCode);
    }
}