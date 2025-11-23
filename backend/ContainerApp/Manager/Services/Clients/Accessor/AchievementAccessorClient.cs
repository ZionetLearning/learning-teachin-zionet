using System.Net;
using Dapr.Client;
using Manager.Constants;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Achievements;

namespace Manager.Services.Clients.Accessor;

public class AchievementAccessorClient(
    ILogger<AchievementAccessorClient> logger,
    DaprClient daprClient
    ) : IAchievementAccessorClient
{
    private readonly ILogger<AchievementAccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;

    public async Task<IReadOnlyList<AchievementAccessorModel>> GetAllActiveAchievementsAsync(CancellationToken ct = default)
    {
        try
        {
            var achievements = await _daprClient.InvokeMethodAsync<List<AchievementAccessorModel>>(
                HttpMethod.Get, AppIds.Accessor, "achievements-accessor", ct);

            _logger.LogInformation("Retrieved {Count} achievements from accessor", achievements?.Count ?? 0);
            return achievements ?? new List<AchievementAccessorModel>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all achievements");
            throw;
        }
    }

    public async Task<IReadOnlyDictionary<Guid, DateTime>> GetUserUnlockedAchievementsAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var unlockedAchievements = await _daprClient.InvokeMethodAsync<List<AchievementAccessorModel>>(
                HttpMethod.Get, AppIds.Accessor, $"achievements-accessor/user/{userId}/unlocked", ct);

            _logger.LogInformation("Retrieved {Count} unlocked achievements for user {UserId}", unlockedAchievements?.Count ?? 0, userId);

            return unlockedAchievements?.ToDictionary(a => a.AchievementId, a => a.CreatedAt)
                ?? new Dictionary<Guid, DateTime>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unlocked achievements for user {UserId}", userId);
            throw;
        }
    }

    public async Task UnlockAchievementAsync(Guid userId, Guid achievementId, CancellationToken ct = default)
    {
        try
        {
            var request = new UnlockAchievementAccessorRequest(userId, achievementId);
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post, AppIds.Accessor, "achievements-accessor/unlock", request, ct);

            _logger.LogInformation("Unlocked achievement {AchievementId} for user {UserId}", achievementId, userId);
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogInformation("Achievement {AchievementId} already unlocked for user {UserId}", achievementId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking achievement {AchievementId} for user {UserId}", achievementId, userId);
            throw;
        }
    }

    public async Task<UserProgressAccessorModel?> GetUserProgressAsync(Guid userId, string feature, CancellationToken ct = default)
    {
        try
        {
            return await _daprClient.InvokeMethodAsync<UserProgressAccessorModel?>(
                HttpMethod.Get, AppIds.Accessor, $"achievements-accessor/user/{userId}/progress/{feature}", ct);
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            var sanitizedFeature = feature?.Replace("\r", string.Empty).Replace("\n", string.Empty);
            _logger.LogInformation("No progress found for user {UserId} and feature {Feature}", 
                userId, sanitizedFeature);
            return null;
        }
        catch (Exception ex)
        {
            var sanitizedFeature = feature?.Replace("\r", string.Empty).Replace("\n", string.Empty);
            _logger.LogError(ex, "Error getting progress for user {UserId} and feature {Feature}", 
                userId, sanitizedFeature);
            throw;
        }
    }

    public async Task UpdateUserProgressAsync(Guid userId, UpdateUserProgressAccessorRequest request, CancellationToken ct = default)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put, AppIds.Accessor, $"achievements-accessor/user/{userId}/progress", request, ct);

            var sanitizedFeature = request.Feature?.Replace("\r", string.Empty).Replace("\n", string.Empty);
            _logger.LogInformation("Updated progress for user {UserId}, feature {Feature} to count {Count}", 
                userId, sanitizedFeature, request.Count);
        }
        catch (Exception ex)
        {
            var sanitizedFeature = request.Feature?.Replace("\r", string.Empty).Replace("\n", string.Empty);
            _logger.LogError(ex, "Error updating progress for user {UserId} and feature {Feature}", 
                userId, sanitizedFeature);
            throw;
        }
    }
}
