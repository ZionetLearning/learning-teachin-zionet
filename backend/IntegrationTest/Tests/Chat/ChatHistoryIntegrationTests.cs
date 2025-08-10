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
    private readonly ITestOutputHelper _output;

    public ChatHistoryIntegrationTests(AccessorHttpTestFixture fixture, ITestOutputHelper output)
        : base(fixture)
    {
        _output = output;
    }

    [Fact(DisplayName = "POST /chat-history/message => 201 + created message with UTC timestamp (no thread in payload)")]
    public async Task Post_Message_Should_Return_Created_And_Utc_NoThread()
    {
        var threadId = Guid.NewGuid();

        var req = new ChatMessageRequest
        {
            ThreadId = threadId,
            UserId = "alice",
            Role = "user",                // server maps to enum
            Content = "Hello, how are you?"
        };

        var response = await Client.PostAsJsonAsync(ApiRoutes.ChatMessage, req);
        response.ShouldBeCreated();

        var msg = await response.Content.ReadFromJsonAsync<ChatMessageDto>();
        msg.Should().NotBeNull();
        msg!.Id.Should().NotBeEmpty();
        msg.ThreadId.Should().Be(threadId);
        msg.UserId.Should().Be("alice");
        msg.Content.Should().Be("Hello, how are you?");
        // Role may be "User" (enum string). Accept either case depending on converter.
        msg.Role.Should().BeOneOf("User", "user");

        // must be UTC (Z)
        msg.Timestamp.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact(DisplayName = "GET /chat-history/{threadId} => returns only messages for that thread (ordered by time)")]
    public async Task Get_History_By_Thread_Should_Return_Only_Thread_Messages()
    {
        var threadId = Guid.NewGuid();

        // post 2 messages
        await PostMessageAsync(threadId, "alice", "user", "Hello, how are you? 1");
        await PostMessageAsync(threadId, "alice", "user", "Hello, how are you? 2");

        var historyResp = await Client.GetAsync(ApiRoutes.ChatHistoryByThread(threadId));
        historyResp.EnsureSuccessStatusCode();

        var history = await historyResp.Content.ReadFromJsonAsync<List<ChatMessageDto>>();
        history.Should().NotBeNull();
        history!.Should().NotBeEmpty();
        history.Should().OnlyContain(m => m.ThreadId == threadId);
        history.Should().BeInAscendingOrder(m => m.Timestamp); // stable sort on server

        // UTC check
        history.Select(m => m.Timestamp.Offset).Should().OnlyContain(o => o == TimeSpan.Zero);
    }

    [Fact(DisplayName = "GET /chat-history/threads/{userId} => returns thread summaries for user (no messages)")]
    public async Task Get_Threads_By_User_Should_Return_Summaries()
    {
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();

        // create two threads for alice by posting messages
        await PostMessageAsync(t1, "alice", "user", "Hi 1");
        await PostMessageAsync(t2, "alice", "user", "Hi 2");

        var resp = await Client.GetAsync(ApiRoutes.ChatThreadsByUser("alice"));
        resp.EnsureSuccessStatusCode();

        var threads = await resp.Content.ReadFromJsonAsync<List<ThreadSummaryDto>>();
        threads.Should().NotBeNull();
        threads!.Select(t => t.ThreadId).Should().Contain(new[] { t1, t2 });

        // optional sanity checks
        threads.Should().OnlyContain(t => t.ChatType == "default");
        threads.Select(t => t.CreatedAt.Offset).Should().OnlyContain(o => o == TimeSpan.Zero);
        threads.Select(t => t.UpdatedAt.Offset).Should().OnlyContain(o => o == TimeSpan.Zero);
    }

    private async Task<ChatMessageDto> PostMessageAsync(Guid threadId, string userId, string role, string content)
    {
        var req = new ChatMessageRequest { ThreadId = threadId, UserId = userId, Role = role, Content = content };
        var resp = await Client.PostAsJsonAsync(ApiRoutes.ChatMessage, req);
        resp.EnsureSuccessStatusCode();
        var msg = await resp.Content.ReadFromJsonAsync<ChatMessageDto>();
        msg.Should().NotBeNull();
        return msg!;
    }
}
