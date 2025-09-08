using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using IntegrationTests.Models.Notification;
using Manager.Models.Chat;
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
    protected async Task<TaskModel> CreateTaskAsync(TaskModel? task = null)
    {
        task ??= TestDataHelper.CreateRandomTask();

        OutputHelper.WriteLine($"Creating task with ID: {task.Id}, Name: {task.Name}");

        var response = await PostAsJsonAsync(ApiRoutes.Task, task);
        response.EnsureSuccessStatusCode();
        OutputHelper.WriteLine($"Response status code: {response.StatusCode}");

        var receivedNotification = await WaitForNotificationAsync(
           n => n.Type == NotificationType.Success &&
           n.Message.Contains(task.Name),
           TimeSpan.FromSeconds(10));
        receivedNotification.Should().NotBeNull();

        OutputHelper.WriteLine($"Received notification: {receivedNotification.Notification.Message}");

        await TaskUpdateHelper.WaitForTaskNameUpdateAsync(Client, task.Id, task.Name);

        OutputHelper.WriteLine(
            $"Task created successfully with status code: {response.StatusCode}"
        );
        return task;
    }

    protected async Task<HttpResponseMessage> UpdateTaskNameAsync(int id, string newName)
    {
        OutputHelper.WriteLine($"Updating task ID {id} with new name: {newName}");

        var response = await Client.PutAsync(ApiRoutes.UpdateTaskName(id, newName), null);

        OutputHelper.WriteLine($"Update response status: {response.StatusCode}");
        return response;
    }

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

    protected static string? GetAssistantMessage(ReceivedEvent ev) =>
        ev.Event.Payload.TryGetProperty("assistantMessage", out var msgEl)
            ? msgEl.GetString()
            : null;

    protected static bool TryGetPayloadProp(ReceivedEvent ev, string name, out JsonElement value)
    {
        var p = ev.Event.Payload;
        if (p.ValueKind == JsonValueKind.Object && p.TryGetProperty(name, out value))
            return true;
        var alt = char.IsLower(name[0])
            ? char.ToUpperInvariant(name[0]) + name.Substring(1)
            : char.ToLowerInvariant(name[0]) + name.Substring(1);
        return p.TryGetProperty(alt, out value);
    }

    public async Task<ReceivedEvent?> WaitForChatAiAnswerAsync(string requestId, TimeSpan? timeout = null) =>
await WaitForEventAsync(
    e => e.EventType == EventType.ChatAiAnswer &&
         e.Payload.ValueKind == JsonValueKind.Object &&
         e.Payload.TryGetProperty("requestId", out var rid) &&
         rid.GetString() == requestId,
    timeout);
}