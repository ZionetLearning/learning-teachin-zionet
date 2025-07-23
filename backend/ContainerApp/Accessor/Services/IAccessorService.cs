using Accessor.Models;

namespace Accessor.Services
{
    public interface IAccessorService
    {
        Task<TaskModel?> GetTaskByIdAsync(int id);
        Task SaveTaskAsync(TaskModel task);
        Task InitializeAsync();
        Task<bool> DeleteTaskAsync(int taskId);
        Task<bool> UpdateTaskNameAsync(int taskId, string newName);

    }
}
