using Accessor.ComponentTests;
using Accessor.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AccessorComponentTests;
public class ChatEndpointsComponentTests : IClassFixture<ChatEndpointsFactory>
{
    private readonly ChatEndpointsFactory _factory;
    private HttpClient Client => _factory.Client;
    public ChatEndpointsComponentTests(ChatEndpointsFactory factory) => _factory = factory;

    [Fact(DisplayName = "POST /threads/message → 201; then GET /threads/{threadId}/messages returns it")]
    public async Task Post_Then_Get_History_Works()
    {
        var threadId = Guid.NewGuid();

        var request = new ChatMessage
        {
            ThreadId = threadId,
            UserId = "alice",
            Role = MessageRole.User,
            Content = "Hello from component test"
        };

        var post = await Client.PostAsJsonAsync("/threads/message", request);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var history = await Client.GetAsync($"/threads/{threadId}/messages");
        history.StatusCode.Should().Be(HttpStatusCode.OK);

        var items = await history.Content.ReadFromJsonAsync<List<ChatMessage>>();
        items.Should().NotBeNull();
        items!.Should().ContainSingle();
        items[0].Content.Should().Be("Hello from component test");
        items[0].Role.Should().Be(MessageRole.User);
        items[0].ThreadId.Should().Be(threadId);
    }

    [Fact(DisplayName = "GET /threads/{userId} → returns user thread summaries (after a message)")]
    public async Task Get_Threads_For_User_Works()
    {
        var threadId = Guid.NewGuid();

        // Seed via the API (so it uses the same path as production)
        var msg = new ChatMessage
        {
            ThreadId = threadId,
            UserId = "bob",
            Role = MessageRole.User,
            Content = "First message"
        };
        var post = await Client.PostAsJsonAsync("/threads/message", msg);
        post.StatusCode.Should().Be(HttpStatusCode.Created);

        var res = await Client.GetAsync($"/threads/{msg.UserId}");
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var threads = await res.Content.ReadFromJsonAsync<List<ThreadSummaryDto>>();
        threads.Should().NotBeNull();
        threads!.Should().ContainSingle(t => t.ThreadId == threadId);
    }
}
