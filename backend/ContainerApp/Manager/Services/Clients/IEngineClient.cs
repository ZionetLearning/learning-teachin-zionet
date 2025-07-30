using Manager.Models;

namespace Manager.Services.Clients;

public interface IEngineClient
{
    Task<(bool success, string message)> ProcessTaskAsync(TaskModel task);
}
