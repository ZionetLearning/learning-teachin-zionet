using Manager.Models;

namespace Manager.Services
{
    public interface IManagerService
    {
        Task<TaskModel?> GetTaskAsync(int id);
        Task<(bool success, string message)> ProcessTaskAsync(TaskModel task);
    }
}
