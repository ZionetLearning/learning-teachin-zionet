
namespace Accessor.Services
{
    public interface IAccessorService
    {
        Task InitializeAsync();
        Task<bool> DeleteTaskAsync(int taskId);
        Task<bool> UpdateTaskNameAsync(int taskId, string newName);

    }
}
