using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Chat;
public class ChatHistoryIntegrationTests : AccessorIntegrationTestBase
{
    private readonly AccessorHttpTestFixture fixture;
    private readonly ITestOutputHelper _output;

    public ChatHistoryIntegrationTests(AccessorHttpTestFixture fixture, ITestOutputHelper output)
        : base(fixture)
    {
        _output = output;
    }


    [Fact(DisplayName = "POST /chat-history/message => 201 + created message with thread and UTC timestamp")]
    public async Task Post_Message_Should_Return_Created_And_Thread()
    {
        var threadId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var req = new ChatMessageRequest
        {
            ThreadId = threadId,
            UserId = "alice",
            Role = "user",
            Content = "Hello, how are you?"
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ChatMessage, req);
        response.ShouldBeCreated();

        var msg = await response.Content.ReadFromJsonAsync<ChatMessageDto>();
        msg.Should().NotBeNull();
        msg!.Id.Should().NotBeEmpty();
        msg.ThreadId.Should().Be(threadId);
        msg.UserId.Should().Be("alice");
        msg.Role.Should().Be("user");
        msg.Content.Should().Be("Hello, how are you?");
        msg.Timestamp.Offset.Should().Be(TimeSpan.Zero);                   // UTC (Z)

        msg.Thread.Should().NotBeNull();
        msg.Thread!.ThreadId.Should().Be(threadId);
        msg.Thread!.UserId.Should().Be("alice");
    }

    [Fact(DisplayName = "GET /chat-history/{threadId} => returns all messages for that thread")]
    public async Task Get_History_By_Thread_Should_Return_All_Messages()
    {
        var threadId = Guid.NewGuid();

        // post 2 messages
        await PostMessageAsync(threadId, "alice", "user", "Hello, how are you? 1");
        await PostMessageAsync(threadId, "alice", "user", "Hello, how are you? 2");

        var historyResp = await Client.GetAsync(ApiRoutes.ChatHistoryByThread(threadId));
        historyResp.EnsureSuccessStatusCode();
        var history = await historyResp.Content.ReadFromJsonAsync<List<ChatMessageDto>>();

        history.Should().NotBeNull();
        history.Should().NotBeNull();
        history!.All(m => m.ThreadId == threadId).Should().BeTrue();
        history!.All(m => m.Thread != null && m.Thread!.UserId == "alice").Should().BeTrue();
    }

    [Fact(DisplayName = "GET /chat-history/threads/{userId} => returns all messages for user (any thread)")]
    public async Task Get_Messages_By_User_Should_Return_All()
    {
        var threadId = Guid.NewGuid();

        // post 2 messages for alice
        var m1 = await PostMessageAsync(threadId, "alice", "user", "Hi 1");
        var m2 = await PostMessageAsync(threadId, "alice", "user", "Hi 2");

        var userResp = await Client.GetAsync(ApiRoutes.ChatMessagesByUser("alice"));
        userResp.EnsureSuccessStatusCode();

        var items = await userResp.Content.ReadFromJsonAsync<List<ChatMessageDto>>();
        items.Should().NotBeNull();
        items!.Should().Contain(x => x.Id == m1.Id);
        items!.Should().Contain(x => x.Id == m2.Id);
        items!.All(x => x.Thread != null && x.Thread!.UserId == "alice").Should().BeTrue();
    }

    private async Task<ChatMessageDto> PostMessageAsync(Guid threadId, string userId, string role, string content)
    {
        var req = new ChatMessageRequest { ThreadId = threadId, UserId = userId, Role = role, Content = content };
        var resp = await Client.PostAsJsonAsync(ApiRoutes.ChatMessage, req);
        resp.EnsureSuccessStatusCode();
        var msg = await resp.Content.ReadFromJsonAsync<ChatMessageDto>();
        msg!.Should().NotBeNull();
        return msg!;
    }
}
