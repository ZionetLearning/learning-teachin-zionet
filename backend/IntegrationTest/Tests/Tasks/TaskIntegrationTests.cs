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
) : TaskTestBase(sharedFixture, sharedFixture.HttpFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    [Fact(DisplayName = "POST /tasks-manager/task - Same ID twice still returns 202 Accepted")]
    public async Task Post_Same_Id_Twice_Should_Return_Accepted()
    {
        var first  = TestDataHelper.CreateFixedIdTask(1001);
        var second = TestDataHelper.CreateFixedIdTask(1001); // same Id

        // 1) POST first
        var r1 = await Client.PostAsJsonAsync(ApiRoutes.Task, first);
        r1.ShouldBeAccepted();

        // Prefer notification, but don't fail if it doesn't arrive in time
        var received = await TryWaitForNotificationAsync(
            n => n.Type == NotificationType.Success && n.Message.Contains(first.Name),
            TimeSpan.FromSeconds(20)
        );
        if (received is null)
            OutputHelper.WriteLine("No SignalR notification within timeout; proceeding via HTTP polling.");

        // 2) Confirm the first write happened (ground truth)
        var before = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, first.Id, timeoutSeconds: 30);
        before.Name.Should().Be(first.Name);
        before.Payload.Should().Be(first.Payload);

        // 3) POST duplicate (same Id) — Manager still returns 202 Accepted
        var r2 = await Client.PostAsJsonAsync(ApiRoutes.Task, second);
        r2.ShouldBeAccepted();

        // 4) Confirm nothing changed
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

        receivedNotification.Should().NotBeNull("Expected a success notification for task creation");
        OutputHelper.WriteLine($"Received notification: {receivedNotification.Notification.Message}");
    }

    [Theory(DisplayName = "POST /tasks-manager/task - With invalid task should return 400 BadRequest")]
    [MemberData(nameof(TaskTestData.InvalidTasks), MemberType = typeof(TaskTestData))]
    public async Task Post_Invalid_Task_Should_Return_BadRequest(TaskModel? invalidTask)
    {
        OutputHelper.WriteLine("Running: Post_Invalid_Task_Should_Return_BadRequest");

        var response = await Client.PostAsJsonAsync(ApiRoutes.Task, invalidTask);
        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "POST /tasks-manager/task - Same ID with different payload should still return 202 Accepted")]
    public async Task Post_Same_Id_With_Different_Payload_Should_Return_Accepted()
    {
        var first = TestDataHelper.CreateFixedIdTask(2002);
        var second = TestDataHelper.CreateFixedIdTask(2002);
        second.Name = first.Name + "_changed"; // different payload

        // 1) POST first
        var r1 = await Client.PostAsJsonAsync(ApiRoutes.Task, first);
        r1.ShouldBeAccepted();

        // 2) POST second with same Id but different payload — Manager doesn’t check, still Accepted
        var r2 = await Client.PostAsJsonAsync(ApiRoutes.Task, second);
        r2.ShouldBeAccepted();
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
