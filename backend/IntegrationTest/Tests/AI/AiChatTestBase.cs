using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models.Notification;
using Manager.Models.Chat;
using Manager.Models.Users;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;


namespace IntegrationTests.Tests.AI;

public abstract class AiChatTestBase(
    HttpTestFixture fixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(fixture, outputHelper, signalRFixture)
{

    protected async Task<(string RequestId, ReceivedEvent Event, string AssistantMessage, string ChatName)>
        PostChatAndWaitAsync(ChatRequest request, TimeSpan? timeout = null)
    {
        var resp = await Client.PostAsJsonAsync(AiRoutes.PostNewMessage, request);
        resp.EnsureSuccessStatusCode();

        var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
        doc.TryGetProperty("requestId", out var ridEl).Should().BeTrue();
        var requestId = ridEl.GetString();
        requestId.Should().NotBeNullOrWhiteSpace();

        var ev = await WaitForChatAiAnswerAsync(requestId!, timeout);
        ev.Should().NotBeNull();

        var payload = ev!.Event.Payload;

        payload.TryGetProperty("assistantMessage", out var msgEl).Should().BeTrue();
        var msg = msgEl.GetString() ?? string.Empty;

        payload.TryGetProperty("chatName", out var nameEl).Should().BeTrue();
        var chatName = nameEl.GetString() ?? string.Empty;

        return (requestId!, ev!, msg, chatName);
    }

    public async Task<ReceivedEvent?> WaitForChatAiAnswerAsync(string requestId, TimeSpan? timeout = null) =>
await WaitForEventAsync(
    e => e.EventType == EventType.ChatAiAnswer &&
         e.Payload.ValueKind == JsonValueKind.Object &&
         e.Payload.TryGetProperty("requestId", out var rid) &&
         rid.GetString() == requestId,
    timeout);


    public async Task UpdateUserInterestsAsync(Guid userId, IEnumerable<string> interests, CancellationToken ct = default)
    {
        var payload = new UpdateUserModel
        {
            Role = Role.Student
        };

        var response = await Client.PutAsJsonAsync($"users-manager/user/{userId}", payload, ct);

        response.EnsureSuccessStatusCode();
    }
}