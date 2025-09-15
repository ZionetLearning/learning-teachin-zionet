using System.Text.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using Accessor.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Endpoints;

public class AccessorEndpointsTests
{
    private static Mock<ITaskService> TaskSvc() => new(MockBehavior.Strict);
    private static Mock<IChatService> ChatSvc() => new(MockBehavior.Strict);
    private static Mock<ILogger<TaskService>> TaskLog() => new();
    private static Mock<ILogger<ChatService>> ChatLog() => new();

    #region GetTaskByIdAsync

    [Fact]
    public async Task GetTaskByIdAsync_Found_ReturnsOk()
    {
        var svc = TaskSvc();
        var log = TaskLog();
        var http = new DefaultHttpContext();
        var response = http.Response;

        var task = new TaskModel { Id = 7, Name = "hello" };
        svc.Setup(s => s.GetTaskWithEtagAsync(7))
           .ReturnsAsync((task, "123"));

        var result = await TasksEndpoints.GetTaskByIdAsync(7, svc.Object, log.Object, response);

        var ok = result.Should().BeOfType<Ok<TaskModel>>().Subject;
        ok.Value.Should().Be(task);
        response.Headers.ETag.ToString().Should().Be("\"123\"");
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetTaskByIdAsync_NotFound_Returns404()
    {
        var svc = TaskSvc();
        var log = TaskLog();
        var response = new DefaultHttpContext().Response;

        svc.Setup(s => s.GetTaskWithEtagAsync(99))
           .ReturnsAsync(((TaskModel Task, string ETag)?)null);

        var result = await TasksEndpoints.GetTaskByIdAsync(99, svc.Object, log.Object, response);

        result.Should().BeOfType<NotFound<string>>();
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetTaskByIdAsync_Exception_ReturnsProblem()
    {
        var svc = TaskSvc();
        var log = TaskLog();
        var response = new DefaultHttpContext().Response;

        svc.Setup(s => s.GetTaskWithEtagAsync(1))
           .ThrowsAsync(new InvalidOperationException("boom"));

        var result = await TasksEndpoints.GetTaskByIdAsync(1, svc.Object, log.Object, response);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    #endregion

    #region CreateTaskAsync

    [Fact]
    public async Task CreateTaskAsync_Success_ReturnsCreated()
    {
        var svc = TaskSvc();
        var log = TaskLog();

        var model = new TaskModel { Id = 1, Name = "n" };
        svc.Setup(s => s.CreateTaskAsync(model)).Returns(Task.CompletedTask);

        var result = await TasksEndpoints.CreateTaskAsync(model, svc.Object, log.Object, CancellationToken.None);

        var created = result.Should().BeOfType<Created<TaskModel>>().Subject;
        created.Value.Should().BeEquivalentTo(model);

        svc.VerifyAll();
    }

    [Fact]
    public async Task CreateTaskAsync_Failure_ReturnsProblem()
    {
        var svc = TaskSvc();
        var log = TaskLog();

        var model = new TaskModel { Id = 1, Name = "n" };
        svc.Setup(s => s.CreateTaskAsync(model)).ThrowsAsync(new Exception("nope"));

        var result = await TasksEndpoints.CreateTaskAsync(model, svc.Object, log.Object, CancellationToken.None);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    #endregion

    #region UpdateTaskNameAsync

    [Theory]
    [InlineData(5, "new", true, false)]
    [InlineData(5, "new", false, true)]
    [InlineData(99, "new", false, false)]
    public async Task UpdateTaskNameAsync_ReturnsExpectedResult(
        int id, string name, bool updated, bool preconditionFailed)
    {
        var svc = TaskSvc();
        var log = TaskLog();
        var response = new DefaultHttpContext().Response;

        var req = new UpdateTaskName { Id = id, Name = name };

        var resultFromSvc = new UpdateTaskResult(
            Updated: updated,
            NotFound: (!updated && !preconditionFailed && id == 99),
            PreconditionFailed: preconditionFailed,
            NewEtag: updated ? "456" : null
        );

        svc.Setup(s => s.UpdateTaskNameAsync(id, name, "\"123\""))
           .ReturnsAsync(resultFromSvc);

        var result = await TasksEndpoints.UpdateTaskNameAsync(
            req,
            ifMatch: "\"123\"",
            taskService: svc.Object,          // <-- fixed name
            logger: log.Object,
            response: response
        );

        if (updated)
        {
            var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
            status.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Headers.ETag.ToString().Should().Be("\"456\"");
        }
        else if (preconditionFailed)
        {
            var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
            status.StatusCode.Should().Be(StatusCodes.Status412PreconditionFailed);
        }
        else
        {
            result.Should().BeOfType<NotFound<string>>();
        }

        svc.VerifyAll();
    }

    [Fact]
    public async Task UpdateTaskNameAsync_Exception_ReturnsProblem()
    {
        var svc = TaskSvc();
        var log = TaskLog();
        var response = new DefaultHttpContext().Response;

        var req = new UpdateTaskName { Id = 5, Name = "new" };

        svc.Setup(s => s.UpdateTaskNameAsync(5, "new", "\"1\""))
           .ThrowsAsync(new Exception("oops"));

        var result = await TasksEndpoints.UpdateTaskNameAsync(
            req, "\"1\"", svc.Object, log.Object, response);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    #endregion

    #region UpsertHistorySnapshotAsync

    [Fact]
    public async Task UpsertHistorySnapshotAsync_BadRequest_When_ThreadId_Missing()
    {
        var svc = ChatSvc();
        var log = ChatLog();

        var body = new UpsertHistoryRequest
        {
            ThreadId = Guid.Empty,
            UserId = Guid.NewGuid(),
            ChatType = null,
            History = default
        };

        var result = await InvokeUpsertHistorySnapshotAsync(body, svc.Object, log.Object);

        var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        status.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var val = result.Should().BeAssignableTo<IValueHttpResult>().Subject.Value!;
        var json = JsonSerializer.Serialize(val);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.GetProperty("error").GetString().Should().NotBeNullOrWhiteSpace();

        svc.VerifyNoOtherCalls();
    }

    private static Task<IResult> InvokeUpsertHistorySnapshotAsync(
        UpsertHistoryRequest body, IChatService s, ILogger<ChatService> l)
        => (Task<IResult>)typeof(ChatsEndpoints)
            .GetMethod("UpsertHistorySnapshotAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { body, s, l })!;

    #endregion

    #region GetHistorySnapshotAsync

    private static Task<IResult> InvokeGetHistorySnapshotAsync(
        Guid threadId, Guid userId, IChatService s, ILogger<ChatService> l)
        => (Task<IResult>)typeof(ChatsEndpoints)
            .GetMethod("GetHistorySnapshotAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { threadId, userId, s, l })!;

    #endregion

    #region GetThreadsForUserAsync

    [Fact]
    public async Task GetThreadsForUserAsync_Exception_ReturnsProblem()
    {
        var svc = ChatSvc();
        var log = ChatLog();
        var userId = Guid.NewGuid();

        svc.Setup(s => s.GetChatsForUserAsync(userId)).ThrowsAsync(new Exception("x"));

        var result = await InvokeGetThreadsForUserAsync(userId, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    private static Task<IResult> InvokeGetThreadsForUserAsync(Guid user, IChatService s, ILogger<ChatService> l)
        => (Task<IResult>)typeof(ChatsEndpoints)
            .GetMethod("GetChatsForUserAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { user, s, l })!;

    #endregion
}
