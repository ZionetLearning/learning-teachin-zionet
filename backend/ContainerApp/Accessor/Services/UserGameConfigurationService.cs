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

        var existingConfig = await _db.UserGameConfigs
            .FirstOrDefaultAsync(x => x.UserId == userGameConfig.UserId && x.GameName == userGameConfig.GameName, ct);

        if (existingConfig != null)
        {
            existingConfig.Difficulty = userGameConfig.Difficulty;
            existingConfig.Nikud = userGameConfig.Nikud;
            existingConfig.NumberOfSentences = userGameConfig.NumberOfSentences;

            _db.UserGameConfigs.Update(existingConfig);
        }
        else
        {
            await _db.UserGameConfigs.AddAsync(userGameConfig, ct);
        }

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Game config saved successfully for UserId={UserId}, GameName={GameName}", userGameConfig.UserId, userGameConfig.GameName);
    }

    public async Task DeleteConfigAsync(Guid userId, GameName gameName, CancellationToken ct)
    {
        _logger.LogInformation("Deleting game config for UserId={UserId}, GameName={GameName}", userId, gameName);

        var removeConfig = await GetGameConfigAsync(userId, gameName, ct);
        if (removeConfig is null)
        {
            _logger.LogWarning("Game config not found for deletion: UserId={UserId}, GameName={GameName}", userId, gameName);
            return;
        }

        _db.UserGameConfigs.Remove(removeConfig);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Game config deleted successfully: UserId={UserId}, GameName={GameName}", userId, gameName);
    }
}
