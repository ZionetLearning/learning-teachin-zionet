using Engine.Models;

namespace Engine.Services
{
    public interface IEngineService
    {
        Task ProcessTaskAsync(TaskModel task);
    }
}
