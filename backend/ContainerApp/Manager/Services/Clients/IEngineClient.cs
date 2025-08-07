using Manager.Models;

namespace Manager.Services.Clients;

public interface IEngineClient
{
    Task<(bool success, string message, int? taskId)> ProcessTaskAsync(TaskModel task, string idempotencyKey);
}
