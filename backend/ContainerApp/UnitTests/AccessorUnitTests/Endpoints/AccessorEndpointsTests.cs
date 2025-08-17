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

    #region StoreMessageAsync

    [Fact]
    public async Task StoreMessageAsync_BadRequest_When_Content_Missing_Or_Role_Invalid()
    {
        var svc = Svc();
        var log = Log();

        // invalid: empty content
        var msg1 = new ChatMessage
        {
            ThreadId = Guid.NewGuid(),
            Content = "",
            Role = 0
        };

        var r1 = await InvokeStoreMessageAsync(msg1, svc.Object, log.Object);
        r1.Should().BeOfType<BadRequest<string>>();

        // invalid: whitespace content
        var msg2 = new ChatMessage
        {
            ThreadId = Guid.NewGuid(),
            Content = "   ",
            Role = (MessageRole)123 // definitely invalid enum value
        };

        var r2 = await InvokeStoreMessageAsync(msg2, svc.Object, log.Object);
        r2.Should().BeOfType<BadRequest<string>>();

        svc.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task StoreMessageAsync_Success_Sets_Id_And_Timestamp_And_Returns_CreatedAtRoute()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        ChatMessage? saved = null;

        svc.Setup(s => s.AddMessageAsync(It.IsAny<ChatMessage>()))
           .Callback<ChatMessage>(m => saved = m)
           .Returns(Task.CompletedTask);

        var msg = new ChatMessage
        {
            ThreadId = threadId,
            Content = "hello",
            Role = MessageRole.User
        };

        var result = await InvokeStoreMessageAsync(msg, svc.Object, log.Object);

        var created = result.Should().BeOfType<CreatedAtRoute<ChatMessage>>().Subject;
        created.RouteName.Should().Be("GetChatHistory");
        created.RouteValues.Should().ContainKey("threadId").WhoseValue.Should().Be(threadId);
        created.Value.Should().NotBeNull();

        // server-side mutation checks
        saved.Should().NotBeNull();
        saved!.Id.Should().NotBe(Guid.Empty);
        saved.Timestamp.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1)); // sanity
        saved.ThreadId.Should().Be(threadId);

        svc.VerifyAll();
    }

    [Fact]
    public async Task StoreMessageAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        svc.Setup(s => s.AddMessageAsync(It.IsAny<ChatMessage>()))
           .ThrowsAsync(new Exception("db down"));

        var msg = new ChatMessage
        {
            ThreadId = Guid.NewGuid(),
            Content = "x",
            Role = MessageRole.Assistant
        };

        var result = await InvokeStoreMessageAsync(msg, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    private static Task<IResult> InvokeStoreMessageAsync(ChatMessage m, IAccessorService s, ILogger<AccessorService> l)
        => (Task<IResult>)typeof(AccessorEndpoints)
            .GetMethod("StoreMessageAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { m, s, l })!;

    #endregion

    #region GetChatHistoryAsync

    [Fact]
    public async Task GetChatHistoryAsync_ThreadExists_ReturnsMessages()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        svc.Setup(s => s.GetThreadByIdAsync(threadId)).ReturnsAsync(new ChatThread { ThreadId = threadId });
        var messages = new List<ChatMessage> {
            new ChatMessage { ThreadId = threadId, Content = "a", Role = MessageRole.User },
            new ChatMessage { ThreadId = threadId, Content = "b", Role = MessageRole.Assistant }
        };
        svc.Setup(s => s.GetMessagesByThreadAsync(threadId)).ReturnsAsync(messages);

        var result = await InvokeGetChatHistoryAsync(threadId, svc.Object, log.Object);

        var ok = result.Should().BeOfType<Ok<IEnumerable<ChatMessage>>>().Subject;
        ok.Value.Should().BeEquivalentTo(messages);

        svc.VerifyAll();
    }

    [Fact]
    public async Task GetChatHistoryAsync_ThreadMissing_AutoCreates_And_ReturnsEmpty()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        svc.Setup(s => s.GetThreadByIdAsync(threadId)).ReturnsAsync((ChatThread?)null);
        svc.Setup(s => s.CreateThreadAsync(It.Is<ChatThread>(t => t.ThreadId == threadId)))
           .Returns(Task.CompletedTask);

        var result = await InvokeGetChatHistoryAsync(threadId, svc.Object, log.Object);

        var ok = result.Should().BeOfType<Ok<ChatMessage[]>>().Subject;
        ok.Value.Should().BeEmpty();

        svc.VerifyAll();
    }

    [Fact]
    public async Task GetChatHistoryAsync_Exception_ReturnsProblem()
    {
        var svc = Svc();
        var log = Log();

        var threadId = Guid.NewGuid();
        svc.Setup(s => s.GetThreadByIdAsync(threadId)).ThrowsAsync(new Exception("boom"));

        var result = await InvokeGetChatHistoryAsync(threadId, svc.Object, log.Object);

        result.Should().BeOfType<ProblemHttpResult>();
        svc.VerifyAll();
    }

    private static Task<IResult> InvokeGetChatHistoryAsync(Guid id, IAccessorService s, ILogger<AccessorService> l)
        => (Task<IResult>)typeof(AccessorEndpoints)
            .GetMethod("GetChatHistoryAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
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
            .GetMethod("GetThreadsForUserAsync", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { user, s, l })!;
    #endregion
}
