using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Fixtures;
using IntegrationTests.Helpers;
using IntegrationTests.Models;
using IntegrationTests.Models.Notification;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Tasks;

[Collection("Shared test collection")]
public class TaskIntegrationTests(
    SharedTestFixture sharedFixture,
    ITestOutputHelper outputHelper,
    SignalRTestFixture signalRFixture
) : TaskTestBase(sharedFixture.HttpFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    private readonly SharedTestFixture _shared = sharedFixture;

    public override async Task InitializeAsync()
    {
        await _shared.GetAuthenticatedTokenAsync();

        await _shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
    }

    [Fact(DisplayName = "POST /tasks-manager/task - Same ID twice is idempotent (second POST is a no-op)")]
    public async Task Post_Same_Id_Twice_Is_Idempotent()
    {
        var first = TestDataHelper.CreateFixedIdTask(); // Id = 888
        var second = TestDataHelper.CreateFixedIdTask(); // same Id

        // 1) POST first
        var r1 = await Client.PostAsJsonAsync(ApiRoutes.Task, first);
        r1.ShouldBeAccepted();

        var receivedNotification = await WaitForNotificationAsync(
            n => n.Type == NotificationType.Success && n.Message.Contains(first.Name),
            TimeSpan.FromSeconds(10)
        );

        receivedNotification.Should().NotBeNull("Expected a success notification for task creation");
        OutputHelper.WriteLine($"Received notification: {receivedNotification.Notification.Message}");

        // 2) Wait until it's visible
        var before = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, first.Id, timeoutSeconds: 20);
        before.Name.Should().Be(first.Name);
        before.Payload.Should().Be(first.Payload);

        // 3) POST duplicate (same Id)
        var r2 = await Client.PostAsJsonAsync(ApiRoutes.Task, second);
        r2.ShouldBeAccepted();

        // 4) Confirm it did NOT change
        var after = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, first.Id, timeoutSeconds: 10);
        after.Name.Should().Be(first.Name);
        after.Payload.Should().Be(first.Payload);
    }

    [Fact(DisplayName = "POST /tasks-manager/task - With valid task should return 202 Accepted")]
    public async Task Post_Valid_Task_Should_Return_Accepted()
    {
        OutputHelper.WriteLine("Running: Post_Valid_Task_Should_Return_Accepted");

        var task = TestDataHelper.CreateRandomTask();
        OutputHelper.WriteLine($"Creating task with ID {task.Id} and name {task.Name}");

        var response = await PostAsJsonAsync(ApiRoutes.Task, task);
        response.ShouldBeAccepted();

        // Wait for signalR notification
        var receivedNotification = await WaitForNotificationAsync(
            n => n.Type == NotificationType.Success && n.Message.Contains(task.Name),
            TimeSpan.FromSeconds(10)
        );

        receivedNotification
            .Should()
            .NotBeNull("Expected a success notification for task creation");
        OutputHelper.WriteLine(
            $"Received notification: {receivedNotification.Notification.Message}"
        );

        var result = await ReadAsJsonAsync<TaskModel>(response);
        result.Should().NotBeNull();

        OutputHelper.WriteLine($"Response location header: {response.Headers.Location}");
        OutputHelper.WriteLine("Task creation succeeded");
    }

    [Theory(DisplayName = "POST /tasks-manager/task - With invalid task should return 400 Bad Request")]
    [MemberData(nameof(TaskTestData.InvalidTasks), MemberType = typeof(TaskTestData))]
    public async Task Post_Invalid_Task_Should_Return_BadRequest(TaskModel? invalidTask)
    {
        OutputHelper.WriteLine("Running: Post_Invalid_Task_Should_Return_BadRequest");

        var response = await Client.PostAsJsonAsync(ApiRoutes.Task, invalidTask);
        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "GET /tasks-manager/task/{id} - With valid ID should return task")]
    public async Task Get_Task_By_Valid_Id_Should_Return_Task()
    {
        OutputHelper.WriteLine("Running: Get_Task_By_Valid_Id_Should_Return_Task");

        var task = await CreateTaskAsync();
        OutputHelper.WriteLine($"Created task with ID: {task.Id}");

        var fetchedTask = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, task.Id);

        fetchedTask.Id.Should().Be(task.Id);
        fetchedTask.Name.Should().Be(task.Name);
        fetchedTask.Payload.Should().Be(task.Payload);

        OutputHelper.WriteLine($"Verified task: ID={fetchedTask.Id}, Name={fetchedTask.Name}");
    }

    [Theory(DisplayName = "GET /tasks-manager/task/{id} - Invalid ID should return 404 Not Found")]
    [InlineData(-1)]
    public async Task Get_Task_By_Invalid_Id_Should_Return_NotFound(int invalidId)
    {
        OutputHelper.WriteLine($"Running: Get_Task_By_Invalid_Id_Should_Return_NotFound for ID {invalidId}");

        var response = await Client.GetAsync(ApiRoutes.TaskById(invalidId));
        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "PUT /tasks-manager/task/{id}/{name} - Valid update should return 200 OK")]
    public async Task Put_TaskName_With_Valid_Id_Should_Update_Name()
    {
        OutputHelper.WriteLine("Running: Put_TaskName_With_Valid_Id_Should_Update_Name");

        var task = await CreateTaskAsync();
        var newName = "Updated-Name";

        await TaskUpdateHelper.WaitForTaskByIdAsync(Client, task.Id);

        OutputHelper.WriteLine($"Updating task {task.Id} name to '{newName}'");

        var response = await UpdateTaskNameAsync(task.Id, newName);
        response.ShouldBeOk();

        await TaskUpdateHelper.WaitForTaskNameUpdateAsync(Client, task.Id, newName);

        var getResponse = await Client.GetAsync(ApiRoutes.TaskById(task.Id));
        var updated = await ReadAsJsonAsync<TaskModel>(getResponse);

        updated!.Name.Should().Be(newName);
        OutputHelper.WriteLine($"Successfully updated task name to: {updated.Name}");
    }

    [Fact(DisplayName = "PUT /tasks-manager/task/{id}/{name} - Invalid ID should return 404 Not Found")]
    public async Task Put_TaskName_With_Invalid_Id_Should_Return_NotFound()
    {
        OutputHelper.WriteLine("Running: Put_TaskName_With_Invalid_Id_Should_Return_NotFound");

        var response = await UpdateTaskNameAsync(-1, "DoesNotMatter");
        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "DELETE /tasks-manager/task/{id} - With valid ID should delete task")]
    public async Task Delete_Task_With_Valid_Id_Should_Succeed()
    {
        OutputHelper.WriteLine("Running: Delete_Task_With_Valid_Id_Should_Succeed");

        var task = await CreateTaskAsync();
        OutputHelper.WriteLine($"Deleting task ID: {task.Id}");

        var deleteResponse = await Client.DeleteAsync(ApiRoutes.TaskById(task.Id));
        deleteResponse.ShouldBeOk();

        await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, task.Id);

        OutputHelper.WriteLine("Verified task deletion");
    }

    [Fact(DisplayName = "DELETE /tasks-manager/task/{id} - With invalid ID should return 404")]
    public async Task Delete_Task_With_Invalid_Id_Should_Return_NotFound()
    {
        OutputHelper.WriteLine("Running: Delete_Task_With_Invalid_Id_Should_Return_NotFound");

        var response = await Client.DeleteAsync(ApiRoutes.TaskById(-1));
        response.ShouldBeNotFound();
    }
}