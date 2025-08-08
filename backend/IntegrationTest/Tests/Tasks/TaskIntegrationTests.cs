using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.Constants;
using IntegrationTests.Helpers;
using IntegrationTests.Infrastructure;
using IntegrationTests.Models;
using Xunit.Abstractions;

namespace IntegrationTests.Tests.Tasks;

[Collection("IntegrationTests")]
public class TaskIntegrationTests(HttpTestFixture fixture, ITestOutputHelper outputHelper)
    : TaskTestBase(fixture, outputHelper)
{

    [Trait("Integration", "Task")]
    [Fact(DisplayName = "POST /task - With idempotency should return same result")]
    public async Task PostTask_WithIdempotency_ShouldReturnSameResponse()
    {
        OutputHelper.WriteLine("Starting PostTask_WithIdempotency_ShouldReturnSameResponse test");

        var task = TestDataHelper.CreateRandomTask();
        var requestId = Guid.NewGuid().ToString();
        OutputHelper.WriteLine($"Using Idempotency-Key: {requestId}");

        //First request
        var request1 = new HttpRequestMessage(HttpMethod.Post, "/task")
        {
            Content = JsonContent.Create(task)
        };
        request1.Headers.Add("Idempotency-Key", requestId);

        var response1 = await Client.SendAsync(request1);
        response1.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var result1 = await ReadAsJsonAsync<Dictionary<string, object>>(response1);
        result1.Should().ContainKey("status");
        var status1 = result1["status"]?.ToString();
        OutputHelper.WriteLine($"First response status: {status1}");

        //Second request with same ID
        var request2 = new HttpRequestMessage(HttpMethod.Post, "/task")
        {
            Content = JsonContent.Create(task)
        };
        request2.Headers.Add("Idempotency-Key", requestId);

        var response2 = await Client.SendAsync(request2);
        response2.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var result2 = await ReadAsJsonAsync<Dictionary<string, object>>(response2);
        result2.Should().ContainKey("status");
        var status2 = result2["status"]?.ToString();
        OutputHelper.WriteLine($"Second response status: {status2}");

        //Assert both responses are the same or consistent
        status2.Should().Be("AlreadyProcessed");
        result2["id"]?.ToString().Should().Be(task.Id.ToString());

        OutputHelper.WriteLine("Idempotent POST test passed successfully");
    }

    [Fact(DisplayName = "POST /task - With valid task should return 202 Accepted")]
    public async Task Post_Valid_Task_Should_Return_Accepted()
    {
        OutputHelper.WriteLine("Running: Post_Valid_Task_Should_Return_Accepted");

        var task = TestDataHelper.CreateRandomTask();
        OutputHelper.WriteLine($"Creating task with ID {task.Id} and name {task.Name}");

        var headers = new Dictionary<string, string> { { "Idempotency-Key", Guid.NewGuid().ToString() } };
        var response = await PostAsJsonAsync(ApiRoutes.Task, task, headers);    
        response.ShouldBeAccepted();

        var result = await ReadAsJsonAsync<TaskModel>(response);
        result.Should().NotBeNull();

        OutputHelper.WriteLine($"Response location header: {response.Headers.Location}");
        OutputHelper.WriteLine("Task creation succeeded");
    }

    [Theory(DisplayName = "POST /task - With invalid task should return 400 Bad Request")]
    [MemberData(nameof(TaskTestData.InvalidTasks), MemberType = typeof(TaskTestData))]
    public async Task Post_Invalid_Task_Should_Return_BadRequest(TaskModel? invalidTask)
    {
        OutputHelper.WriteLine("Running: Post_Invalid_Task_Should_Return_BadRequest");

        var response = await Client.PostAsJsonAsync(ApiRoutes.Task, invalidTask);
        response.ShouldBeBadRequest();
    }

    [Fact(DisplayName = "GET /task/{id} - With valid ID should return task")]
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

    [Theory(DisplayName = "GET /task/{id} - Invalid ID should return 404 Not Found")]
    [InlineData(-1)]
    public async Task Get_Task_By_Invalid_Id_Should_Return_NotFound(int invalidId)
    {
        OutputHelper.WriteLine($"Running: Get_Task_By_Invalid_Id_Should_Return_NotFound for ID {invalidId}");

        var response = await Client.GetAsync(ApiRoutes.TaskById(invalidId));
        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "PUT /task/{id}/{name} - Valid update should return 200 OK")]
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

    [Fact(DisplayName = "PUT /task/{id}/{name} - Invalid ID should return 404 Not Found")]
    public async Task Put_TaskName_With_Invalid_Id_Should_Return_NotFound()
    {
        OutputHelper.WriteLine("Running: Put_TaskName_With_Invalid_Id_Should_Return_NotFound");

        var response = await UpdateTaskNameAsync(-1, "DoesNotMatter");
        response.ShouldBeNotFound();
    }

    [Fact(DisplayName = "DELETE /task/{id} - With valid ID should delete task")]
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

    [Fact(DisplayName = "DELETE /task/{id} - With invalid ID should return 404")]
    public async Task Delete_Task_With_Invalid_Id_Should_Return_NotFound()
    {
        OutputHelper.WriteLine("Running: Delete_Task_With_Invalid_Id_Should_Return_NotFound");

        var response = await Client.DeleteAsync(ApiRoutes.TaskById(-1));
        response.ShouldBeNotFound();
    }
}
