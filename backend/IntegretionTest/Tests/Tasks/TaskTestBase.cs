using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Tasks;

public abstract class TaskTestBase : IntegrationTestBase
{
    protected readonly ITestOutputHelper OutputHelper;

    protected TaskTestBase(HttpTestFixture fixture, ITestOutputHelper outputHelper) : base(fixture) 
    {
        OutputHelper = outputHelper;
    }

    protected async Task<TaskModel> CreateTaskAsync(TaskModel? task = null)
    {
        try
        {
            task ??= TestDataHelper.CreateValidTask();

            OutputHelper.WriteLine($"Creating task: {task.Name} with ID: {task.Id}");

            var response = await PostAsJsonAsync("/task", task);
            response.EnsureSuccessStatusCode();

            OutputHelper.WriteLine($"Task created successfully. Status: {response.StatusCode}");
            return task;
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine($"Failed to create task: {ex.Message}");
            throw new Exception($"Failed to create task: {ex.Message}", ex);
        }
    }
}
