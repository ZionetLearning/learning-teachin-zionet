using IntegrationTests.Constants;
using IntegrationTests.Models;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.Helpers;

public static class TaskUpdateHelper
{
    public static async Task WaitForTaskNameUpdateAsync(HttpClient client, int taskId, string expectedName, int timeoutSeconds = 5)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync(ApiRoutes.TaskById(taskId));
            if (response.IsSuccessStatusCode)
            {
                var task = await response.Content.ReadFromJsonAsync<TaskModel>();
                if (task?.Name == expectedName)
                    return;
            }

            await Task.Delay(300);
        }

        throw new TimeoutException($"Timed out waiting for task {taskId} to be updated to '{expectedName}'.");
    }

    public static async Task WaitForTaskDeletionAsync(HttpClient client, int taskId, int timeoutSeconds = 5)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync(ApiRoutes.TaskById(taskId));
            if (response.StatusCode == HttpStatusCode.NotFound)
                return;

            await Task.Delay(300);
        }

        throw new TimeoutException("Timed out waiting for task to be deleted.");
    }

    public static async Task<TaskModel> WaitForTaskByIdAsync(HttpClient client, int taskId, int timeoutSeconds = 5)
    {
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            var response = await client.GetAsync(ApiRoutes.TaskById(taskId));
            if (response.IsSuccessStatusCode)
            {
                var task = await response.Content.ReadFromJsonAsync<TaskModel>();
                if (task != null)
                    return task;
            }

            await Task.Delay(300); // retry
        }

        throw new TimeoutException($"Timed out waiting for task with ID {taskId} to become available.");
    }
}
