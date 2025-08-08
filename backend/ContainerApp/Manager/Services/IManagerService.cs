using Manager.Models;

namespace Manager.Services;

public interface IManagerService
{
    Task<bool> UpdateTaskName(int id, string newTaskName);
    Task<bool> DeleteTask(int id);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<(bool success, string message, int? taskId)> ProcessTaskAsync(TaskModel task, string idempotencyKey);
}
