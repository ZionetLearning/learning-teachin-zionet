using Manager.Models;

namespace Manager.Services.Clients;

public interface IAccessorClient
{
    public Task<bool> UpdateTaskName(int id, string newTaskName);
    public Task<bool> DeleteTask(int id);
    Task<TaskModel?> GetTaskAsync(int id);
}
