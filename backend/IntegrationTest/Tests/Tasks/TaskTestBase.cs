using IntegrationTests.Constants;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Tasks;

public abstract class TaskTestBase : IntegrationTestBase
{
    protected readonly ITestOutputHelper OutputHelper;

    protected TaskTestBase(HttpTestFixture fixture, ITestOutputHelper outputHelper)
        : base(fixture)
    {
        OutputHelper = outputHelper;
    }

    protected async Task<TaskModel> CreateTaskAsync(TaskModel? task = null)
    {
        task ??= TestDataHelper.CreateRandomTask();

        OutputHelper.WriteLine($"Creating task with ID: {task.Id}, Name: {task.Name}");

        var response = await PostAsJsonAsync(ApiRoutes.Task, task);
        response.EnsureSuccessStatusCode();

        await TaskUpdateHelper.WaitForTaskNameUpdateAsync(Client, task.Id, task.Name);

        OutputHelper.WriteLine($"Task created successfully with status code: {response.StatusCode}");
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
