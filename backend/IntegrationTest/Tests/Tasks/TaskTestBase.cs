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
           TimeSpan.FromSeconds(60));
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
}
