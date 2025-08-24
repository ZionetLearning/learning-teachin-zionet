using System.Text.Json;
using Accessor.Endpoints;
using Accessor.Models;
using Accessor.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;

namespace AccessorUnitTests.Endpoints;

public class AccessorEndpointsTests
{
    private static Mock<IAccessorService> Svc() => new(MockBehavior.Strict);
    private static Mock<ILogger<AccessorService>> Log() => new();

    #region GetTaskByIdAsync

    [Fact]
    public async Task GetTaskByIdAsync_Found_ReturnsOk()
    {
        var svc = Svc();
        var log = Log();

        var task = new TaskModel { Id = 7, Name = "hello" };
        svc.Setup(s => s.GetTaskByIdAsync(7)).ReturnsAsync(task);

        var result = await AccessorEndpoints.GetTaskByIdAsync(7, svc.Object, log.Object);

        var ok = result.Should().BeOfType<Ok<TaskModel>>().Subject;
        ok.Value.Should().Be(task);
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetTaskByIdAsync_NotFound_Returns404()
    {
        var svc = Svc();
        var log = Log();

        svc.Setup(s => s.GetTaskByIdAsync(99)).ReturnsAsync((TaskModel?)null);

        var result = await AccessorEndpoints.GetTaskByIdAsync(99, svc.Object, log.Object);

        result.Should().BeOfType<NotFound<string>>();
        svc.VerifyAll();
    }

    [Fact]
    public async Task GetTaskByIdAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        svc.Setup(s => s.GetTaskByIdAsync(1)).ThrowsAsync(new InvalidOperationException("boom"));

        var result = await AccessorEndpoints.GetTaskByIdAsync(1, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    #endregion

    #region CreateTaskAsync

    [Fact]
    public async Task CreateTaskAsync_Success_ReturnsOk()
    {
        var svc = Svc();
        var log = Log();

        var model = new TaskModel { Id = 1, Name = "n" };
        svc.Setup(s => s.CreateTaskAsync(model)).Returns(Task.CompletedTask);

        var result = await AccessorEndpoints.CreateTaskAsync(model, svc.Object, log.Object, CancellationToken.None);

        var ok = result.Should().BeOfType<Ok<string>>().Subject;
        ok.Value.Should().Contain("Task 1 Saved");
        svc.VerifyAll();
    }

    [Fact]
    public async Task CreateTaskAsync_Failure_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        var model = new TaskModel { Id = 1, Name = "n" };
        svc.Setup(s => s.CreateTaskAsync(model)).ThrowsAsync(new Exception("nope"));

        var result = await AccessorEndpoints.CreateTaskAsync(model, svc.Object, log.Object, CancellationToken.None);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    #endregion

    #region UpdateTaskNameAsync

    [Theory]
    [InlineData(5, "new", true, typeof(Ok<string>))]
    [InlineData(99, "new", false, typeof(NotFound<string>))]
    public async Task UpdateTaskNameAsync_ReturnsExpectedResult(
        int id, string name, bool found, Type expectedType)
    {
        var svc = Svc();
        var log = Log();

        var req = new UpdateTaskName { Id = id, Name = name };
        svc.Setup(s => s.UpdateTaskNameAsync(id, name)).ReturnsAsync(found);

        var result = await AccessorEndpoints.UpdateTaskNameAsync(req, svc.Object, log.Object);

        result.Should().BeOfType(expectedType);
        svc.VerifyAll();
    }

    [Fact]
    public async Task UpdateTaskNameAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        var req = new UpdateTaskName { Id = 5, Name = "new" };
        svc.Setup(s => s.UpdateTaskNameAsync(5, "new")).ThrowsAsync(new Exception("oops"));

        var result = await AccessorEndpoints.UpdateTaskNameAsync(req, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    #endregion

    #region DeleteTaskAsync

    [Theory]
    [InlineData(3, true, typeof(Ok<string>))]
    [InlineData(3, false, typeof(NotFound<string>))]
    public async Task DeleteTaskAsync_ReturnsExpectedResult(int id, bool found, Type expectedType)
    {
        var svc = Svc();
        var log = Log();

        svc.Setup(s => s.DeleteTaskAsync(id)).ReturnsAsync(found);

        var result = await AccessorEndpoints.DeleteTaskAsync(id, svc.Object, log.Object);

        result.Should().BeOfType(expectedType);
        svc.VerifyAll();
    }

    [Fact]
    public async Task DeleteTaskAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        svc.Setup(s => s.DeleteTaskAsync(3)).ThrowsAsync(new Exception("err"));

        var result = await AccessorEndpoints.DeleteTaskAsync(3, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    #endregion

    #region UpsertHistorySnapshotAsync

    [Fact]
    public async Task UpsertHistorySnapshotAsync_BadRequest_When_ThreadId_Missing()
    {
        var svc = Svc();
        var log = Log();

        var body = new UpsertHistoryRequest
        {
            ThreadId = Guid.Empty,
            UserId = "u",
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

    [Fact]
    public async Task UpsertHistorySnapshotAsync_BadRequest_When_UserId_Missing()
    {
        var svc = Svc();
        var log = Log();

        using var docIn = JsonDocument.Parse("""{"messages":[]}""");
        var body = new UpsertHistoryRequest
        {
            ThreadId = Guid.NewGuid(),
            UserId = "",
            ChatType = null,
            History = docIn.RootElement.Clone()
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

    [Fact]
    public async Task UpsertHistorySnapshotAsync_BadRequest_When_History_Missing()
    {
        var svc = Svc();
        var log = Log();

        var body = new UpsertHistoryRequest
        {
            ThreadId = Guid.NewGuid(),
            UserId = "u",
            ChatType = "default",
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

    [Fact]
    public async Task UpsertHistorySnapshotAsync_Existing_Updated_Returns200_And_PreservesCreatedAt()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);

        var existing = new ChatHistorySnapshot
        {
            ThreadId = threadId,
            UserId = "old",
            ChatType = "default",
            History = """{"messages":[]}""",
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        svc.Setup(s => s.GetHistorySnapshotAsync(threadId)).ReturnsAsync(existing);

        ChatHistorySnapshot? saved = null;
        svc.Setup(s => s.UpsertHistorySnapshotAsync(It.IsAny<ChatHistorySnapshot>()))
           .Callback<ChatHistorySnapshot>(snp => saved = snp)
           .Returns(Task.CompletedTask);

        using var doc = JsonDocument.Parse("""{"messages":[{"role":"user","content":"new"}]}""");
        var body = new UpsertHistoryRequest
        {
            ThreadId = threadId,
            UserId = "user-2",
            ChatType = "chatty",
            History = doc.RootElement.Clone()
        };

        var result = await InvokeUpsertHistorySnapshotAsync(body, svc.Object, log.Object);

        var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        status.StatusCode.Should().Be(StatusCodes.Status200OK);

        var val = result.Should().BeAssignableTo<IValueHttpResult>().Subject.Value!;
        val.Should().NotBeNull();

        saved.Should().NotBeNull();
        saved!.ThreadId.Should().Be(threadId);
        saved.UserId.Should().Be("user-2");
        saved.ChatType.Should().Be("chatty");
        saved.History.Should().Be(doc.RootElement.GetRawText());
        saved.CreatedAt.Should().Be(createdAt);
        saved.UpdatedAt.Should().BeOnOrAfter(createdAt);

        svc.VerifyAll();
    }

    [Fact]
    public async Task UpsertHistorySnapshotAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        svc.Setup(s => s.GetHistorySnapshotAsync(threadId)).ReturnsAsync((ChatHistorySnapshot?)null);
        svc.Setup(s => s.UpsertHistorySnapshotAsync(It.IsAny<ChatHistorySnapshot>()))
           .ThrowsAsync(new Exception("db fail"));

        using var doc = JsonDocument.Parse("""{"messages":[]}""");
        var body = new UpsertHistoryRequest
        {
            ThreadId = threadId,
            UserId = "u",
            ChatType = "default",
            History = doc.RootElement.Clone()
        };

        var result = await InvokeUpsertHistorySnapshotAsync(body, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    private static Task<IResult> InvokeUpsertHistorySnapshotAsync(
        UpsertHistoryRequest body, IAccessorService s, ILogger<AccessorService> l)
        => (Task<IResult>)typeof(AccessorEndpoints)
            .GetMethod("UpsertHistorySnapshotAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { body, s, l })!;

    #endregion

    #region GetHistorySnapshotAsync

    [Fact]
    public async Task GetHistorySnapshotAsync_SnapshotExists_ReturnsOkWithHistory()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        var json = """{"messages":[{"role":"assistant","content":"yo"}]}""";

        svc.Setup(s => s.GetHistorySnapshotAsync(threadId)).ReturnsAsync(new ChatHistorySnapshot
        {
            ThreadId = threadId,
            UserId = "u1",
            ChatType = "default",
            History = json,
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        });

        var result = await InvokeGetHistorySnapshotAsync(threadId, svc.Object, log.Object);

        var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        status.StatusCode.Should().Be(StatusCodes.Status200OK);

        var value = result.Should().BeAssignableTo<IValueHttpResult>().Subject.Value!;
        var payloadJson = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(payloadJson);
        var root = doc.RootElement;

        root.GetProperty("threadId").GetGuid().Should().Be(threadId);
        root.GetProperty("UserId").GetString().Should().Be("u1");
        root.GetProperty("ChatType").GetString().Should().Be("default");
        var hist = root.GetProperty("history");
        hist.ValueKind.Should().Be(JsonValueKind.Object);
        hist.GetProperty("messages").EnumerateArray().Should().HaveCount(1);

        svc.VerifyAll();
    }

    [Fact]
    public async Task GetHistorySnapshotAsync_SnapshotMissing_ReturnsOkWithEmptyMessages()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        svc.Setup(s => s.GetHistorySnapshotAsync(threadId)).ReturnsAsync((ChatHistorySnapshot?)null);

        var result = await InvokeGetHistorySnapshotAsync(threadId, svc.Object, log.Object);

        var status = result.Should().BeAssignableTo<IStatusCodeHttpResult>().Subject;
        status.StatusCode.Should().Be(StatusCodes.Status200OK);

        var value = result.Should().BeAssignableTo<IValueHttpResult>().Subject.Value!;
        var payloadJson = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(payloadJson);
        var root = doc.RootElement;

        root.GetProperty("threadId").GetGuid().Should().Be(threadId);
        root.GetProperty("userId").ValueKind.Should().Be(JsonValueKind.Null);
        root.GetProperty("chatType").ValueKind.Should().Be(JsonValueKind.Null);

        var hist = root.GetProperty("history");
        hist.ValueKind.Should().Be(JsonValueKind.Object);
        hist.GetProperty("messages").EnumerateArray().Should().BeEmpty();

        svc.VerifyAll();
    }

    [Fact]
    public async Task GetHistorySnapshotAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        svc.Setup(s => s.GetHistorySnapshotAsync(threadId)).ThrowsAsync(new Exception("boom"));

        var result = await InvokeGetHistorySnapshotAsync(threadId, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }
    private static Task<IResult> InvokeGetHistorySnapshotAsync(
        Guid id, IAccessorService s, ILogger<AccessorService> l)
        => (Task<IResult>)typeof(AccessorEndpoints)
            .GetMethod("GetHistorySnapshotAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { id, s, l })!;

    #endregion

    #region GetThreadsForUserAsync

    [Fact]
    public async Task GetThreadsForUserAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        svc.Setup(s => s.GetThreadsForUserAsync("bob")).ThrowsAsync(new Exception("x"));

        var result = await InvokeGetThreadsForUserAsync("bob", svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    private static Task<IResult> InvokeGetThreadsForUserAsync(string user, IAccessorService s, ILogger<AccessorService> l)
        => (Task<IResult>)typeof(AccessorEndpoints)
            .GetMethod("GetThreadsForUserAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { user, s, l })!;


    #endregion
}
