using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Models.QueueMessages;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Tasks;

namespace Manager.Services.Clients.Accessor;

public class TaskAccessorClient : ITaskAccessorClient
{
    private readonly ILogger<TaskAccessorClient> _logger;
    private readonly DaprClient _daprClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TaskAccessorClient(
        ILogger<TaskAccessorClient> logger,
        DaprClient daprClient,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _daprClient = daprClient;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<bool> DeleteTask(int id)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(DeleteTask), nameof(TaskAccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, AppIds.Accessor, $"tasks-accessor/task/{id}");
            _logger.LogDebug("Task {TaskId} deletion request sent to Accessor service", id);

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for deletion (404 from accessor)", id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            throw;
        }
    }

    public async Task<CreateTaskAccessorResponse> PostTaskAsync(CreateTaskAccessorRequest request)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(PostTaskAsync), nameof(TaskAccessorClient));

        try
        {
            var userId = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out _))
            {
                _logger.LogError("Missing or invalid UserId in HttpContext.");
                throw new InvalidOperationException("Authenticated user id is missing or not a valid GUID.");
            }

            // Create a temporary TaskModel for queue message (keeping backward compatibility with queue structure)
            var taskModel = new TaskModel
            {
                Id = request.Id,
                Name = request.Name,
                Payload = request.Payload
            };

            var payload = JsonSerializer.SerializeToElement(taskModel);
            var userContextMetadata = JsonSerializer.SerializeToElement(
                new UserContextMetadata
                {
                    UserId = userId!,
                    MessageId = Guid.NewGuid().ToString()
                }
            );

            var message = new Message
            {
                ActionName = MessageAction.CreateTask,
                Payload = payload,
                Metadata = userContextMetadata
            };
            await _daprClient.InvokeBindingAsync($"{QueueNames.AccessorQueue}-out", "create", message);

            _logger.LogDebug(
                "Task sent to Accessor via binding '{Binding}' for user {UserId}",
                QueueNames.AccessorQueue,
                userId
            );

            return new CreateTaskAccessorResponse
            {
                Success = true,
                Message = "sent to queue",
                Id = request.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task to Accessor");
            throw;
        }
    }

    public async Task<(GetTaskAccessorResponse? Task, string? ETag)> GetTaskWithEtagAsync(int id, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetTaskWithEtagAsync), nameof(TaskAccessorClient));

        try
        {
            var req = _daprClient.CreateInvokeMethodRequest(HttpMethod.Get, AppIds.Accessor, $"tasks-accessor/task/{id}");

            using var resp = await _daprClient.InvokeMethodWithResponseAsync(req, ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Task {TaskId} not found at Accessor", id);
                return (null, null);
            }

            resp.EnsureSuccessStatusCode();

            var etag = resp.Headers.ETag?.Tag?.Trim('"');

            var taskModel = await resp.Content.ReadFromJsonAsync<TaskModel>(cancellationToken: ct);

            if (taskModel is null)
            {
                return (null, null);
            }

            var accessorResponse = new GetTaskAccessorResponse
            {
                Id = taskModel.Id,
                Name = taskModel.Name,
                Payload = taskModel.Payload
            };

            return (accessorResponse, etag);
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Task {TaskId} not found at Accessor (404)", id);
            return (null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to GET task {TaskId} with ETag from Accessor", id);
            throw;
        }
    }

    public async Task<GetTasksAccessorResponse> GetTasksAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetTasksAsync), nameof(TaskAccessorClient));
        try
        {
            var list = await _daprClient.InvokeMethodAsync<List<TaskSummaryDto>>(
                HttpMethod.Get,
                AppIds.Accessor,
                "tasks-accessor/tasks",
                ct);

            _logger.LogInformation("Accessor returned {Count} task summaries", list?.Count ?? 0);

            var tasks = list?.Select(t => new TaskSummaryAccessorDto
            {
                Id = t.Id,
                Name = t.Name
            }).ToList() ?? new List<TaskSummaryAccessorDto>();

            return new GetTasksAccessorResponse
            {
                Tasks = tasks
            };
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Accessor returned 404 for tasks list");
            return new GetTasksAccessorResponse
            {
                Tasks = Array.Empty<TaskSummaryAccessorDto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tasks list from Accessor");
            throw;
        }
    }

    public async Task<UpdateTaskNameAccessorResponse> UpdateTaskNameAsync(int id, string newTaskName, string ifMatch, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateTaskNameAsync), nameof(TaskAccessorClient));

        try
        {
            var req = _daprClient.CreateInvokeMethodRequest(HttpMethod.Patch, AppIds.Accessor, "tasks-accessor/task");
            req.Headers.IfMatch.Clear();
            if (!string.IsNullOrWhiteSpace(ifMatch))
            {
                var tag = ifMatch.Trim().Trim('"');
                req.Headers.IfMatch.Add(new EntityTagHeaderValue($"\"{tag}\""));
            }

            var body = new { id, name = newTaskName };
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _daprClient.InvokeMethodWithResponseAsync(req, ct);

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Task {TaskId} not found for update", id);
                return new UpdateTaskNameAccessorResponse
                {
                    Updated = false,
                    NotFound = true,
                    PreconditionFailed = false,
                    NewEtag = null
                };
            }

            if ((int)resp.StatusCode == StatusCodes.Status412PreconditionFailed)
            {
                _logger.LogWarning("Precondition failed for Task {TaskId} (ETag mismatch)", id);
                return new UpdateTaskNameAccessorResponse
                {
                    Updated = false,
                    NotFound = false,
                    PreconditionFailed = true,
                    NewEtag = null
                };
            }

            resp.EnsureSuccessStatusCode();

            var newEtag = resp.Headers.ETag?.Tag?.Trim('"');
            _logger.LogInformation("Task {TaskId} updated; new ETag {ETag}", id, newEtag);
            return new UpdateTaskNameAccessorResponse
            {
                Updated = true,
                NotFound = false,
                PreconditionFailed = false,
                NewEtag = newEtag
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to PATCH update task {TaskId} at Accessor", id);
            throw;
        }
    }
}
