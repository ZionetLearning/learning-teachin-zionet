using Accessor.Models;

namespace Accessor.Services.Interfaces;

public interface ITaskService
{
    Task<(TaskModel Task, string ETag)?> GetTaskWithEtagAsync(int id);
    Task<TaskModel?> GetTaskByIdAsync(int id);
    Task CreateTaskAsync(TaskModel task);
    Task<UpdateTaskResult> UpdateTaskNameAsync(int taskId, string newName, string? ifMatch);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<IReadOnlyList<TaskWithEtag>> GetAllTasksWithEtagsAsync(CancellationToken ct = default);
}