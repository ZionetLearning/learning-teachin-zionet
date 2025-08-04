using Accessor.Models;

namespace Accessor.Services;

public interface IAccessorService
{
    Task InitializeAsync();
    Task<TaskModel?> GetTaskByIdAsync(int id);
    Task CreateTaskAsync(TaskModel task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<bool> UpdateTaskNameAsync(int taskId, string newName);
}
