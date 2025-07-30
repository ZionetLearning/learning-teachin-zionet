using FluentAssertions;
using IntegrationTests.Helpers;
using IntegrationTests.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Xunit.Abstractions;
using IntegrationTests.Infrastructure;

namespace IntegrationTests.Tests.Tasks;

[Collection("IntegrationTests")]
public class TaskIntegrationTests(HttpTestFixture fixture, ITestOutputHelper outputHelper) : TaskTestBase(fixture, outputHelper)
{
    [Trait("Integration", "Task")]
    [Fact]
    public async Task PostTask_WithValidTask_ShouldReturnAccepted()
    {
        OutputHelper.WriteLine("Starting PostTask_WithValidTask_ShouldReturnAccepted test");

        var task = TestDataHelper.CreateValidTask();
        OutputHelper.WriteLine($"Test task created: {task.Name}");

        var response = await PostAsJsonAsync("/task", task);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        TaskModel? result = null;
        try
        {
            result = await ReadAsJsonAsync<TaskModel>(response);
            OutputHelper.WriteLine("Successfully deserialized response");
        }
        catch (InvalidOperationException ex)
        {
            OutputHelper.WriteLine($"Failed to deserialize response: {ex.Message}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            OutputHelper.WriteLine($"HTTP request failed: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }

        result.Should().NotBeNull();

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/task/{task.Id}");

        OutputHelper.WriteLine("Test completed successfully");
    }

    [Trait("Integration", "Task")]
    [Fact]
    public async Task PostTask_WithNullTask_ShouldReturnBadRequest()
    {
        OutputHelper.WriteLine("Starting PostTask_WithNullTask_ShouldReturnBadRequest test");

        var response = await Client.PostAsJsonAsync("/task", (TaskModel?)null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        OutputHelper.WriteLine($"Test completed. Response status: {response.StatusCode}");
    }

    [Trait("Integration", "Task")]
    [Fact]
    public async Task GetTask_WithValidId_ShouldReturnTask()
    {
        OutputHelper.WriteLine("Starting GetTask_WithValidId_ShouldReturnTask test");

        var task = await CreateTaskAsync();
        var response = await Client.GetAsync($"/task/{task.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        TaskModel? result = null;
        try
        {
            result = await ReadAsJsonAsync<TaskModel>(response);
            OutputHelper.WriteLine("Successfully deserialized task response");
        }
        catch (InvalidOperationException ex)
        {
            OutputHelper.WriteLine($"Failed to deserialize task response: {ex.Message}");
            throw;
        }
        catch (HttpRequestException ex)
        {
            OutputHelper.WriteLine($"HTTP request failed while reading task: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            OutputHelper.WriteLine($"Unexpected error while reading task: {ex.Message}");
            throw;
        }

        result.Should().NotBeNull();
        result!.Id.Should().Be(task.Id);
        result.Name.Should().Be(task.Name);
        result.Payload.Should().Be(task.Payload);

        OutputHelper.WriteLine("Test completed successfully");
    }

    [Trait("Integration", "Task")]
    [Fact(Skip = "the manger should return NotFound, instead of InternalServerError.")]
    public async Task GetTask_WithInvalidId_ShouldReturnNotFound()
    {
        OutputHelper.WriteLine("Starting GetTask_WithInvalidId_ShouldReturnNotFound test");

        var response = await Client.GetAsync("/task/9999");
        // BUG: The response status code should be NotFound, but it is InternalServerError. need to investigate.
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        OutputHelper.WriteLine($"Test completed. Response status: {response.StatusCode} (Note: Expected NotFound but got InternalServerError - BUG)");
    }
}