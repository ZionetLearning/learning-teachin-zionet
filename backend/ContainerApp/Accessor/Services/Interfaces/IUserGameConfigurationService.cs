using Accessor.Models.GameConfiguration;

namespace Accessor.Services.Interfaces;

public interface IUserGameConfigurationService
{
    Task<UserGameConfig?> GetGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct);
    Task SaveConfigAsync(UserGameConfig userGameConfig, CancellationToken ct);
    Task DeleteConfigAsync(Guid userId, GameName gameName, CancellationToken ct);
}
