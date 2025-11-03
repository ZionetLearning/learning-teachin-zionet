using Accessor.DB;
using Accessor.Models.GameConfiguration;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class UserGameConfigurationService : IUserGameConfigurationService
{
    private readonly AccessorDbContext _db;
    private readonly ILogger<UserGameConfigurationService> _logger;

    public UserGameConfigurationService(ILogger<UserGameConfigurationService> logger, AccessorDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<UserGameConfig?> GetGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct)
    {
        _logger.LogInformation("Getting game config for UserId={UserId}, GameName={GameName}", userId, gameName);

        return await _db.UserGameConfigs
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.UserId == userId && c.GameName == gameName, ct);
    }

    public async Task SaveConfigAsync(UserGameConfig userGameConfig, CancellationToken ct)
    {
        _logger.LogInformation("Saving game config for UserId={UserId}, GameName={GameName}", userGameConfig.UserId, userGameConfig.GameName);

        _db.UserGameConfigs.Update(userGameConfig);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Game config saved successfully for UserId={UserId}, GameName={GameName}", userGameConfig.UserId, userGameConfig.GameName);
    }

    public Task DeleteConfigAsync(Guid userId, GameName gameName, CancellationToken ct)
    {
        _logger.LogInformation("Deleting game config for UserId={UserId}, GameName={GameName}", userId, gameName);

        var removeConfig = GetGameConfigAsync(userId, gameName, ct);
        if (removeConfig is null)
        {
            _logger.LogWarning("Game config not found for deletion: UserId={UserId}, GameName={GameName}", userId, gameName);
        }
        else
        {
            _db.UserGameConfigs.Remove(removeConfig);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Game config deleted successfully: UserId={UserId}, GameName={GameName}", userId, gameName);
        }
    }
}
