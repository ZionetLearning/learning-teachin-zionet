using Accessor.Models;

namespace Accessor.Services;

public interface IAccessorService
{
    Task InitializeAsync();
    Task<TaskModel?> GetTaskByIdAsync(int id);
    Task CreateTaskAsync(TaskModel task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<bool> UpdateTaskNameAsync(int taskId, string newName);
    Task<ChatThread?> GetThreadByIdAsync(Guid threadId);
    Task CreateThreadAsync(ChatThread thread);
    Task AddMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetMessagesByThreadAsync(Guid threadId);
    Task<List<ThreadSummaryDto>> GetThreadsForUserAsync(string userId);
    Task<Guid?> ValidateCredentialsAsync(string email, string password);
}
