using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using IntegrationTests.Models.Notification;
using Manager.Models.Auth;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Tasks;

public abstract class TaskTestBase(
    HttpTestFixture fixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : IntegrationTestBase(fixture, outputHelper, signalRFixture)
{
    protected async Task<string> EnsureAuthenticatedAndGetTokenAsync(string email, string password, CancellationToken ct = default)
    {
        if (Client.DefaultRequestHeaders.Authorization is not null)
        {
            return Client.DefaultRequestHeaders.Authorization.Parameter!;
        }

        var loginResponse = await LoginAsync(email, password, ct);
        loginResponse.EnsureSuccessStatusCode();

        var accessToken = await ExtractAccessToken(loginResponse, ct);
        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return accessToken;

    }
    private async Task<HttpResponseMessage> LoginAsync(string email, string password, CancellationToken ct)
    {
        var req = new LoginRequest { Email = email, Password = password };
        return await Client.PostAsJsonAsync(AuthRoutes.Login, req, ct);
    }

    private static async Task<string> ExtractAccessToken(HttpResponseMessage response, CancellationToken ct)
    {
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync(ct);
        var dto = JsonSerializer.Deserialize<IntegrationTests.Models.Auth.AccessTokenResponse>(body)
                  ?? throw new InvalidOperationException("Invalid JSON response for access token.");
        return dto.AccessToken;
    }

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
}
