using Accessor.Models;

namespace Accessor.Services;

public interface IAccessorService
{
    Task InitializeAsync();
    Task<TaskModel?> GetTaskByIdAsync(int id, IDictionary<string, string>? callbackHeaders = null);
    Task CreateTaskAsync(TaskModel task, IDictionary<string, string>? callbackHeaders = null);
    Task<bool> DeleteTaskAsync(int taskId, IDictionary<string, string>? callbackHeaders = null);
    Task<bool> UpdateTaskNameAsync(int taskId, string newName, IDictionary<string, string>? callbackHeaders = null);
    Task<ChatThread?> GetThreadByIdAsync(Guid threadId);
    Task CreateThreadAsync(ChatThread thread);
    Task AddMessageAsync(ChatMessage message);
    Task<IEnumerable<ChatMessage>> GetMessagesByThreadAsync(Guid threadId);
    Task<List<ThreadSummaryDto>> GetThreadsForUserAsync(string userId);
}
