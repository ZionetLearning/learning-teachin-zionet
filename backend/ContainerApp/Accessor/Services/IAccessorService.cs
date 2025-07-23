using Accessor.Models;

namespace Accessor.Services
{
    public interface IAccessorService
    {
        Task<TaskModel?> GetTaskByIdAsync(int id);
        Task SaveTaskAsync(TaskModel task);
    }
}
