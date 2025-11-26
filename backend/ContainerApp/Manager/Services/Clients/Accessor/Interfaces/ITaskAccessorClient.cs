using Manager.Models;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface ITaskAccessorClient
{
    Task<(TaskModel? Task, string? ETag)> GetTaskWithEtagAsync(int id, CancellationToken ct = default);
    Task<UpdateTaskNameResult> UpdateTaskNameAsync(int id, string newTaskName, string ifMatch, CancellationToken ct = default);
    Task<bool> UpdateTaskName(int id, string newTaskName);
    Task<bool> DeleteTask(int id);
    Task<(bool success, string message)> PostTaskAsync(TaskModel task);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<IReadOnlyList<TaskSummaryDto>> GetTaskSummariesAsync(CancellationToken ct = default);
}