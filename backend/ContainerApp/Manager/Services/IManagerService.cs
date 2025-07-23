using System.Security.Claims;
using static Google.Rpc.Context.AttributeContext.Types;

namespace Manager.Services;

public interface IManagerService
{
    public Task<bool> UpdateTaskName(int id, string newTaskName);
    public Task<bool> DeleteTask(int id);

}
