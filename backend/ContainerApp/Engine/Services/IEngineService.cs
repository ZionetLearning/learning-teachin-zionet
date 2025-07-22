using Engine.Models;

namespace Engine.Services
{
    public interface IEngineService
    {
        Task<List<TaskModel>> GetAllTasksAsync();
        Task ProcessTaskAsync(TaskModel task);
    }
}
