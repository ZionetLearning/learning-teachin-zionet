using Manager.Models.Users;

namespace Manager.Services;

public interface IOnlinePresenceService
{
    Task<(bool first, int count)> AddConnectionAsync(string userId, string name, string role, string connectionId, CancellationToken ct = default);
    Task<(bool last, int count)> RemoveConnectionAsync(string userId, string connectionId, CancellationToken ct = default);
    Task<IReadOnlyList<OnlineUserDto>> GetOnlineAsync(CancellationToken ct = default);
}
