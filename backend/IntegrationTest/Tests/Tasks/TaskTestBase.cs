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

        var notifyTask = WaitForNotificationAsync(
            n =>
                n.Type == NotificationType.Success &&
                (n.Message?.Contains(task.Name, StringComparison.OrdinalIgnoreCase) == true
                 || n.Message?.Contains(task.Id.ToString(), StringComparison.OrdinalIgnoreCase) == true),
            TimeSpan.FromSeconds(20)
        );

        var apiVisibleTask = TaskUpdateHelper.WaitForTaskByIdAsync(Client, task.Id);

        var winner = await Task.WhenAny(notifyTask, apiVisibleTask);

        if (winner == notifyTask)
        {
            var received = await notifyTask;
            received.Should().NotBeNull("Expected a SignalR notification or API visibility");
        }
        else
        {
            OutputHelper.WriteLine("Notification not observed in time; proceeded after API confirmed task existence.");
        }

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
