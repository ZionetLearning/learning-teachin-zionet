using Manager.Models;

namespace Manager.Services;

public interface IManagerCallbacks
{
    Task OnTaskCreatedAsync(TaskResult result);
    Task OnTaskUpdatedAsync(TaskResult result);
}