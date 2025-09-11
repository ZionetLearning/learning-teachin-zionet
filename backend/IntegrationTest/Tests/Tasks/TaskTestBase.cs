using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using IntegrationTests.Models.Notification;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Tasks;

public abstract class TaskTestBase(
    SharedTestFixture sharedFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(sharedFixture.HttpFixture, outputHelper, signalRFixture)
{
    protected SharedTestFixture Shared { get; } = sharedFixture;
    public override Task InitializeAsync() => SuiteInit.EnsureAsync(Shared, SignalRFixture, OutputHelper);

    protected async Task<TaskModel> CreateTaskAsync(TaskModel? task = null)
    {
        task ??= TestDataHelper.CreateRandomTask();

        await Shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
        SignalRFixture.ClearReceivedMessages();

        OutputHelper.WriteLine($"Creating task with ID: {task.Id}, Name: {task.Name}");

        var response = await PostAsJsonAsync(ApiRoutes.Task, task);
        response.EnsureSuccessStatusCode();

        var received = await WaitForNotificationAsync(
            n => n.Type == NotificationType.Success && n.Message.Contains(task.Name),
            TimeSpan.FromSeconds(30)
        );
        received.Should().NotBeNull("Expected a SignalR notification");

        await TaskUpdateHelper.WaitForTaskNameUpdateAsync(Client, task.Id, task.Name);
        return task;
    }

    protected async Task<HttpResponseMessage> UpdateTaskNameAsync(int id, string newName)
    {
        OutputHelper.WriteLine($"Updating task ID {id} with new name: {newName}");

        var response = await Client.PutAsync(ApiRoutes.UpdateTaskName(id, newName), null);

        OutputHelper.WriteLine($"Update response status: {response.StatusCode}");
        return response;
    }
}
