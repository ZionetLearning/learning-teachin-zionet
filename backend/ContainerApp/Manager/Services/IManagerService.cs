using Manager.Models;

namespace Manager.Services;

public interface IManagerService
{
    public Task<bool> UpdateTaskName(int id, string newTaskName);
    public Task<bool> DeleteTask(int id);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<(bool success, string message)> ProcessTaskAsync(TaskModel task);
}
