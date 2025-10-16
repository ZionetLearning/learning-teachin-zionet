using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using IntegrationTests.Models.Notification;
using Manager.Models.Users;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Tasks;

public abstract class TaskTestBase(
    HttpClientFixture httpClientFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(httpClientFixture, outputHelper, signalRFixture)
{
  
    public override async Task InitializeAsync()
    {
        await ClientFixture.LoginAsync(Role.Admin);
        await EnsureSignalRStartedAsync();
    }
    protected async Task<TaskModel> CreateTaskAsync(TaskModel? task = null)
    {
        task ??= TestDataHelper.CreateRandomTask();

        await EnsureSignalRStartedAsync();
        SignalRFixture.ClearReceivedMessages();

        OutputHelper.WriteLine($"Creating task with ID: {task.Id}, Name: {task.Name}");

        var response = await PostAsJsonAsync(ApiRoutes.Task, task);
        response.EnsureSuccessStatusCode();

        var received = await WaitForNotificationAsync(
            n => n.Type == NotificationType.Success && n.Message.Contains(task.Name),
            TimeSpan.FromSeconds(300)
        );
        received.Should().NotBeNull("Expected a SignalR notification");

        await TaskUpdateHelper.WaitForTaskNameUpdateAsync(Client, task.Id, task.Name);
        return task;
    }

    protected async Task<(TaskModel Task, string? ETag)> GetTaskWithEtagAsync(int id)
    {
        var resp = await Client.GetAsync(ApiRoutes.TaskById(id));
        resp.EnsureSuccessStatusCode();

        var task = await ReadAsJsonAsync<TaskModel>(resp);
        var etag = resp.Headers.ETag?.Tag ?? resp.Headers.ETag?.ToString(); // includes quotes
        return (task!, etag);
    }

    protected async Task<HttpResponseMessage> UpdateTaskNameAsync(int id, string newName, string? ifMatch = null)
    {
        // If not provided, fetch current ETag via GET
        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            var (_task, tag) = await GetTaskWithEtagAsync(id);
            ifMatch = tag;
        }

        var req = new HttpRequestMessage(HttpMethod.Put, ApiRoutes.UpdateTaskName(id, newName));
        req.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        return await Client.SendAsync(req);
    }
}
