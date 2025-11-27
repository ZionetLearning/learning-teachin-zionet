using Manager.Services.Clients.Accessor.Models.Tasks;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface ITaskAccessorClient
{
    Task<(GetTaskAccessorResponse? Task, string? ETag)> GetTaskWithEtagAsync(int id, CancellationToken ct = default);
    Task<UpdateTaskNameAccessorResponse> UpdateTaskNameAsync(int id, string newTaskName, string ifMatch, CancellationToken ct = default);
    Task<bool> DeleteTask(int id);
    Task<CreateTaskAccessorResponse> PostTaskAsync(CreateTaskAccessorRequest request);
    Task<GetTasksAccessorResponse> GetTasksAsync(CancellationToken ct = default);
}