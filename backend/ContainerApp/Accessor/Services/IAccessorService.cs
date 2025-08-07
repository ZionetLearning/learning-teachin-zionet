using Accessor.Models;

namespace Accessor.Services;

public interface IAccessorService
{
    Task InitializeAsync();
    Task<TaskModel?> GetTaskByIdAsync(int id);
    Task<(bool success, string message, int? taskId)> CreateTaskAsync(TaskModel task, string idempotencyKey);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<bool> UpdateTaskNameAsync(int taskId, string newName);
}
