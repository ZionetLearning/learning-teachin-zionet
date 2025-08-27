using Manager.Models;
using Manager.Models.Users;

namespace Manager.Services;

public interface IManagerService
{
    Task<bool> UpdateTaskName(int id, string newTaskName);
    Task<bool> DeleteTask(int id);
    Task<TaskModel?> GetTaskAsync(int id);
    Task<(bool success, string message)> CreateTaskAsync(TaskModel task);
    Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task);
    Task SendUserNotificationAsync(string userId, UserNotification notification);
    Task<UserData?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel user);
    Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<UserData>> GetAllUsersAsync();

}
