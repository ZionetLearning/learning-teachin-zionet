using Manager.Models;
using Manager.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Manager.Services;

public class ManagerCallbacks : IManagerCallbacks
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<ManagerCallbacks> _logger;

    public ManagerCallbacks(IHubContext<NotificationHub> hubContext, ILogger<ManagerCallbacks> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task OnTaskCreatedAsync(TaskResult result)
    {
        _logger.LogInformation("[AUTO CALLBACK] Task {Id} created with {Status}", result.Id, result.Status);

        await _hubContext.Clients.All.SendAsync(
            "TaskCreated",
            new TaskUpdateMessage { TaskId = result.Id, Status = result.Status.ToString() }
        );
    }

    public async Task OnTaskUpdatedAsync(TaskResult result)
    {
        _logger.LogInformation("[AUTO CALLBACK] Task {Id} updated with {Status}", result.Id, result.Status);

        await _hubContext.Clients.All.SendAsync(
            "TaskUpdated",
            new TaskUpdateMessage { TaskId = result.Id, Status = result.Status.ToString() }
        );
    }
}