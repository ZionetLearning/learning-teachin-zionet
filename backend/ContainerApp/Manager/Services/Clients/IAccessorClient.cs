using Manager.Models;

namespace Manager.Services.Clients;

public interface IAccessorClient
{
    Task<bool> UpdateTaskName(int id, string newTaskName, IDictionary<string, string>? metadata = null);
    Task<bool> DeleteTask(int id, IDictionary<string, string>? metadata = null);
    Task<TaskModel?> GetTaskAsync(int id, IDictionary<string, string>? metadata = null);
}
