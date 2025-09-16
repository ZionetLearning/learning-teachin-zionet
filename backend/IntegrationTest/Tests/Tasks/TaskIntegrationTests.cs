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
) : TaskTestBase(sharedFixture, outputHelper, signalRFixture), IAsyncLifetime
{
    [Fact(DisplayName = "POST /tasks-manager/task - Same ID twice still returns 202 Accepted")]
    public async Task Post_Same_Id_Twice_Should_Return_Accepted()
    {
        var first = TestDataHelper.CreateFixedIdTask(1001);
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
        await Client.DeleteAsync(ApiRoutes.TaskById(first.Id));
        await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, first.Id);
    }

    [Fact(DisplayName = "POST /tasks-manager/task - With valid task should return 202 Accepted")]
    public async Task Post_Valid_Task_Should_Return_Accepted()
    {
        OutputHelper.WriteLine("Running: Post_Valid_Task_Should_Return_Accepted");

        await Shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
        SignalRFixture.ClearReceivedMessages();

        var task = TestDataHelper.CreateRandomTask();
        OutputHelper.WriteLine($"Creating task with ID {task.Id} and name {task.Name}");

        var response = await PostAsJsonAsync(ApiRoutes.Task, task);
        response.ShouldBeAccepted();

        await Shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);

        var receivedNotification = await WaitForNotificationAsync(
            n => n.Type == NotificationType.Success && n.Message.Contains(task.Name),
            TimeSpan.FromSeconds(20)
        );

        if (receivedNotification is null)
        {
            OutputHelper.WriteLine("No SignalR notification yet; ensuring connection & retrying...");
            await Shared.EnsureSignalRStartedAsync(SignalRFixture, OutputHelper);
            receivedNotification = await WaitForNotificationAsync(
                n => n.Type == NotificationType.Success && n.Message.Contains(task.Name),
                TimeSpan.FromSeconds(10)
            );
        }

        receivedNotification.Should().NotBeNull("Expected a success notification for task creation");
        OutputHelper.WriteLine($"Received notification: {receivedNotification!.Notification.Message}");
        await Client.DeleteAsync(ApiRoutes.TaskById(task.Id));
        await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, task.Id);
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
        var first = TestDataHelper.CreateFixedIdTask(23458);
        var second = TestDataHelper.CreateFixedIdTask(23458);
        second.Name = first.Name + "_changed"; // different payload

        var r1 = await Client.PostAsJsonAsync(ApiRoutes.Task, first);
        r1.ShouldBeAccepted();

        var r2 = await Client.PostAsJsonAsync(ApiRoutes.Task, second);
        r2.ShouldBeAccepted();

        var materialized = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, first.Id, timeoutSeconds: 30);

        await Task.Delay(500);

        var del = await Client.DeleteAsync(ApiRoutes.TaskById(first.Id));
        del.ShouldBeOk();

        try
        {
            await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, first.Id, timeoutSeconds: 20);
        }
        catch (TimeoutException)
        {
            await Task.Delay(500);
            var del2 = await Client.DeleteAsync(ApiRoutes.TaskById(first.Id));
            del2.ShouldBeOk();
            await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, first.Id, timeoutSeconds: 20);
        }
    }
    [Fact(DisplayName = "GET /tasks-manager/tasks - Returns all tasks with per-item ETags")]
    public async Task Get_Tasks_List_Should_Return_All_With_Etags()
    {
        // Arrange: create two tasks
        var t1 = await CreateTaskAsync();
        var t2 = await CreateTaskAsync();

        // (optional) ensure both materialized via helper
        var m1 = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, t1.Id, timeoutSeconds: 30);
        var m2 = await TaskUpdateHelper.WaitForTaskByIdAsync(Client, t2.Id, timeoutSeconds: 30);

        // Act
        var resp = await Client.GetAsync(TestConstants.ListRoute);
        resp.EnsureSuccessStatusCode();

        var list = await resp.Content.ReadFromJsonAsync<List<TaskWithEtagDto>>();
        list.Should().NotBeNull();
        list!.Should().NotBeEmpty();

        // Assert: our two tasks exist in list and have ETags
        var i1 = list.FirstOrDefault(x => x.Task.Id == t1.Id);
        var i2 = list.FirstOrDefault(x => x.Task.Id == t2.Id);

        i1.Should().NotBeNull("newly created task 1 must be in the list");
        i2.Should().NotBeNull("newly created task 2 must be in the list");

        i1!.ETag.Should().NotBeNullOrWhiteSpace("each item must carry an ETag");
        i2!.ETag.Should().NotBeNullOrWhiteSpace();

        // Clean up
        (await Client.DeleteAsync(ApiRoutes.TaskById(t1.Id))).EnsureSuccessStatusCode();
        (await Client.DeleteAsync(ApiRoutes.TaskById(t2.Id))).EnsureSuccessStatusCode();
        await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, t1.Id);
        await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, t2.Id);
    }

    [Fact(DisplayName = "GET /tasks-manager/tasks - Reflects fresh ETag after a single-item update")]
    public async Task Get_Tasks_List_Should_Reflect_Etag_After_Update()
    {
        // Arrange: create one task
        var task = await CreateTaskAsync();

        // Baseline: get current list and pull our item's ETag
        var beforeResp = await Client.GetAsync(TestConstants.ListRoute);
        beforeResp.EnsureSuccessStatusCode();
        var beforeList = await beforeResp.Content.ReadFromJsonAsync<List<TaskWithEtagDto>>();
        beforeList.Should().NotBeNull();
        var beforeItem = beforeList!.FirstOrDefault(x => x.Task.Id == task.Id);
        beforeItem.Should().NotBeNull("created task must appear in list");
        var etagBefore = beforeItem!.ETag;
        etagBefore.Should().NotBeNullOrWhiteSpace();

        // Act: update the task name with If-Match
        var newName = task.Name + "-updated";
        var updateResp = await UpdateTaskNameAsync(task.Id, newName, ifMatch: null /* auto-fetches in helper */);
        updateResp.EnsureSuccessStatusCode();

        // Re-query list
        var afterResp = await Client.GetAsync(TestConstants.ListRoute);
        afterResp.EnsureSuccessStatusCode();
        var afterList = await afterResp.Content.ReadFromJsonAsync<List<TaskWithEtagDto>>();
        afterList.Should().NotBeNull();

        // Assert: item exists, name updated (via eventual consistency helpers), and ETag changed
        var afterItem = afterList!.FirstOrDefault(x => x.Task.Id == task.Id);
        afterItem.Should().NotBeNull();
        afterItem!.Task.Name.Should().Be(newName);
        afterItem.ETag.Should().NotBeNullOrWhiteSpace();
        afterItem.ETag.Should().NotBe(etagBefore, "rowversion/xmin should advance after update");

        // Clean up
        (await Client.DeleteAsync(ApiRoutes.TaskById(task.Id))).EnsureSuccessStatusCode();
        await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, task.Id);
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
        await Client.DeleteAsync(ApiRoutes.TaskById(task.Id));
        await TaskUpdateHelper.WaitForTaskDeletionAsync(Client, task.Id);

    }

    [Theory(DisplayName = "GET /tasks-manager/task/{id} - Invalid ID should return 404 Not Found")]
    [InlineData(-1)]
    public async Task Get_Task_By_Invalid_Id_Should_Return_NotFound(int invalidId)
    {
        OutputHelper.WriteLine($"Running: Get_Task_By_Invalid_Id_Should_Return_NotFound for ID {invalidId}");

        var response = await Client.GetAsync(ApiRoutes.TaskById(invalidId));
        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "PUT /tasks-manager/task/{id}/{name} - Valid update should return 200 OK and new ETag")]
    public async Task Put_TaskName_With_Valid_Id_Should_Update_Name()
    {
        OutputHelper.WriteLine("Running: Put_TaskName_With_Valid_Id_Should_Update_Name");

        var task = await CreateTaskAsync();
        var newName = "Updated-Name";

        var (before, etagBefore) = await GetTaskWithEtagAsync(task.Id);
        etagBefore.Should().NotBeNullOrEmpty("GET should forward an ETag");
        OutputHelper.WriteLine($"Updating task {task.Id} name to '{newName}'");

        var updateResponse = await UpdateTaskNameAsync(task.Id, newName, etagBefore);
        updateResponse.ShouldBeOk();

        var etagAfter = updateResponse.Headers.ETag?.Tag ?? updateResponse.Headers.ETag?.ToString();
        etagAfter.Should().NotBeNullOrEmpty("PUT should return a fresh ETag on success");
        etagAfter.Should().NotBe(etagBefore, "ETag must change after update");

        var (after, etagFromGet) = await GetTaskWithEtagAsync(task.Id);
        after.Name.Should().Be(newName);
        etagFromGet.Should().Be(etagAfter, "GET should reflect the latest ETag after update");
    }

    [Fact(DisplayName = "PUT /tasks-manager/task/{id}/{name} - Missing If-Match returns 428")]
    public async Task Put_TaskName_Without_IfMatch_Returns_428()
    {
        var task = await CreateTaskAsync();
        var req = new HttpRequestMessage(HttpMethod.Put, ApiRoutes.UpdateTaskName(task.Id, "NoHeader"));
        var resp = await Client.SendAsync(req);

        resp.StatusCode.Should().Be(System.Net.HttpStatusCode.PreconditionRequired); // 428
    }

    [Fact(DisplayName = "PUT /tasks-manager/task/{id}/{name} - Stale If-Match returns 412")]
    public async Task Put_TaskName_With_Stale_IfMatch_Returns_412()
    {
        var task = await CreateTaskAsync();

        var (before, etag1) = await GetTaskWithEtagAsync(task.Id);
        var respOk = await UpdateTaskNameAsync(task.Id, "Intermediate-Update", etag1);
        respOk.ShouldBeOk();

        var resp412 = await UpdateTaskNameAsync(task.Id, "Should-412", etag1);
        resp412.StatusCode.Should().Be(System.Net.HttpStatusCode.PreconditionFailed);
    }

    [Fact(DisplayName = "PUT /tasks-manager/task/{id}/{name} - Invalid ID should return 404 Not Found")]
    public async Task Put_TaskName_With_Invalid_Id_Should_Return_NotFound()
    {
        OutputHelper.WriteLine("Running: Put_TaskName_With_Invalid_Id_Should_Return_NotFound");

        var response = await UpdateTaskNameAsync(-1, "DoesNotMatter", "\"dummy-etag\"");
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