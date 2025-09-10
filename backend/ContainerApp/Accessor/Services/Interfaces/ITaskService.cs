using Accessor.Models;

namespace Accessor.Services.Interfaces;

public interface ITaskService
{
    Task<TaskModel?> GetTaskByIdAsync(int id);
    Task CreateTaskAsync(TaskModel task);
    Task<bool> UpdateTaskNameAsync(int taskId, string newName);
    Task<bool> DeleteTaskAsync(int taskId);
}