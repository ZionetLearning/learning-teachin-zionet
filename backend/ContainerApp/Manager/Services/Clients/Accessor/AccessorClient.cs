using System.Net;
using AutoMapper;
using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Manager.Models.Auth;
using Manager.Models.Auth.RefreshSessions;
using Manager.Models.Chat;
using Manager.Models.Classes;
using Manager.Models.UserGameConfiguration;
using Manager.Models.Users;
using Manager.Models.WordCards;
using Manager.Services.Clients.Accessor.Models;
using Manager.Services.Clients.Accessor.Interfaces;

namespace Manager.Services.Clients.Accessor;

public class AccessorClient(
    ILogger<AccessorClient> logger,
    DaprClient daprClient,
    IMapper mapper
    ) : IAccessorClient
{
    private readonly ILogger<AccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;
    private readonly IMapper _mapper = mapper;

    public async Task<int> CleanupRefreshSessionsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(CleanupRefreshSessionsAsync), nameof(AccessorClient));
        try
        {
            var resp = await _daprClient.InvokeMethodAsync<CleanupResponse>(
                HttpMethod.Post,
                AppIds.Accessor,
                "auth-accessor/refresh-sessions/internal/cleanup",
                ct);

            return resp?.Deleted ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to invoke cleanup on Accessor");
            throw;
        }
    }

    public async Task<IReadOnlyList<ChatSummary>> GetChatsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetChatsForUserAsync), nameof(AccessorClient));

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("userId cannot be not Empty.", nameof(userId));
        }

        try
        {
            var chats = await _daprClient.InvokeMethodAsync<List<ChatSummary>>(
                HttpMethod.Get, AppIds.Accessor, $"chats-accessor/{userId}", cancellationToken: ct);

            return chats ?? new List<ChatSummary>();
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No chats found for user {UserId}", userId);
            return Array.Empty<ChatSummary>();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogInformation("{Metod} cancelled for user {UserId}", nameof(GetChatsForUserAsync), userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get chats for user {UserId}", userId);
            throw;
        }
    }

    public async Task<StatsSnapshot?> GetStatsSnapshotAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetStatsSnapshotAsync), nameof(AccessorClient));
        try
        {
            var snapshot = await _daprClient.InvokeMethodAsync<StatsSnapshot>(
                HttpMethod.Get, AppIds.Accessor, "internal-accessor/stats/snapshot", ct);
            return snapshot; // may be null if Accessor returns empty body
        }
        catch (InvocationException ex) when (
            ex.Response?.StatusCode == HttpStatusCode.NoContent ||
            ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No stats snapshot available from Accessor ({Status})", ex.Response?.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats snapshot from Accessor");
            throw;
        }
    }

    public async Task<SpeechTokenResponse> GetSpeechTokenAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetSpeechTokenAsync), nameof(AccessorClient));
        try
        {
            var speechTokenResponse = await _daprClient.InvokeMethodAsync<SpeechTokenResponse>(
                HttpMethod.Get,
                AppIds.Accessor,
                "media-accessor/speech/token",
                ct);
            return speechTokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get speech token from Accessor");
            throw;
        }
    }

    public async Task<AuthenticatedUser?> LoginUserAsync(LoginRequest loginRequest, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(LoginUserAsync), nameof(AccessorClient));
        try
        {
            var response = await _daprClient.InvokeMethodAsync<LoginRequest, AuthenticatedUser?>(
                HttpMethod.Post,
                AppIds.Accessor,
                "auth-accessor/login",
                loginRequest,
                ct
            );
            return response;
        }
        catch (InvocationException ex) when (
            ex.Response?.StatusCode == HttpStatusCode.NoContent ||
            ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                        "Login failed – received {StatusCode} from Accessor", ex.Response?.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get login from the Accessor");
            throw;
        }
    }

    public async Task SaveSessionDBAsync(RefreshSessionRequest session, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(SaveSessionDBAsync), nameof(AccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
            HttpMethod.Post,
            AppIds.Accessor,
            "auth-accessor/refresh-sessions",
            session,
            ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save refresh session to Accessor");
            throw;
        }
    }

    public async Task<RefreshSessionDto> GetSessionAsync(string oldHash, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetSessionAsync), nameof(AccessorClient));
        try
        {
            var session = await _daprClient.InvokeMethodAsync<RefreshSessionDto>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"auth-accessor/refresh-sessions/by-token-hash/{oldHash}",
                ct
            );
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session from the accessor");
            throw;
        }
    }

    public async Task UpdateSessionDBAsync(Guid sessionId, RotateRefreshSessionRequest rotatePayload, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateSessionDBAsync), nameof(AccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
            HttpMethod.Put,
            AppIds.Accessor,
            $"auth-accessor/refresh-sessions/{sessionId}/rotate",
            rotatePayload,
            ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save refresh session to Accessor");
            throw;
        }
    }

    public async Task DeleteSessionDBAsync(Guid sessionId, CancellationToken ct)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(DeleteSessionDBAsync), nameof(AccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
            HttpMethod.Delete,
            AppIds.Accessor,
            $"auth-accessor/refresh-sessions/{sessionId}",
            ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save refresh session to Accessor");
            throw;
        }
    }

    public async Task<IReadOnlyList<WordCard>> GetWordCardsAsync(Guid userId, CancellationToken ct)
    {
        _logger.LogInformation("Fetching word cards for user {UserId}", userId);

        try
        {
            var wordCards = await _daprClient.InvokeMethodAsync<List<WordCard>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"wordcards-accessor/{userId}",
                cancellationToken: ct
            );

            return wordCards ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch word cards for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WordCard> CreateWordCardAsync(Guid userId, CreateWordCardRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating word card for user {UserId}", userId);

        try
        {
            var payload = new CreateWordCard
            {
                UserId = userId,
                Hebrew = request.Hebrew,
                English = request.English,
                Explanation = request.Explanation,
            };

            var response = await _daprClient.InvokeMethodAsync<CreateWordCard, WordCard>(
                HttpMethod.Post,
                AppIds.Accessor,
                $"wordcards-accessor",
                payload,
                cancellationToken: ct
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create word card for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WordCardLearnedStatus> UpdateLearnedStatusAsync(Guid userId, LearnedStatus request, CancellationToken ct)
    {
        _logger.LogInformation("Updating learned status. UserId={UserId}, CardId={CardId}, IsLearned={IsLearned}", userId, request.CardId, request.IsLearned);

        try
        {
            var payload = new SetLearnedStatus
            {
                UserId = userId,
                CardId = request.CardId,
                IsLearned = request.IsLearned,
            };

            var response = await _daprClient.InvokeMethodAsync<SetLearnedStatus, WordCardLearnedStatus>(
                HttpMethod.Patch,
                AppIds.Accessor,
                $"wordcards-accessor/learned",
                payload,
                cancellationToken: ct
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update learned status for CardId={CardId}", request.CardId);
            throw;
        }
    }

    public async Task<ClassDto?> GetClassAsync(Guid classId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching class {ClassId} from Accessor", classId);

        try
        {
            var cls = await _daprClient.InvokeMethodAsync<ClassDto?>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"classes-accessor/{classId:D}",
                ct
            );

            return cls;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Class {ClassId} not found (404)", classId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch class {ClassId} from Accessor", classId);
            throw;
        }
    }
    public async Task<List<ClassDto?>?> GetMyClassesAsync(Guid callerId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching classes for {CallerId} from Accessor", callerId);

        try
        {
            var cls = await _daprClient.InvokeMethodAsync<List<ClassDto?>?>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"classes-accessor/my/{callerId:D}",
                ct
            );

            return cls;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Classes for user {CallerId} not found (404)", callerId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch classes for user {CallerId} from Accessor", callerId);
            throw;
        }
    }
    public async Task<List<ClassDto?>?> GetAllClassesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching class from Accessor");

        try
        {
            var cls = await _daprClient.InvokeMethodAsync<List<ClassDto?>?>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"classes-accessor/",
                ct
            );

            return cls;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Classes not found (404)");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch classes from Accessor");
            throw;
        }
    }
    public async Task<Class?> CreateClassAsync(CreateClassRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating class {Name} via Accessor", request.Name);

        try
        {
            var cls = await _daprClient.InvokeMethodAsync<CreateClassRequest, Class?>(
                HttpMethod.Post,
                AppIds.Accessor,
                "classes-accessor",
                request,
                ct
            );

            return cls;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Class {Name} already exists (409)", request.Name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create class {Name}", request.Name);
            throw;
        }
    }

    public async Task<bool> AddMembersToClassAsync(Guid classId, AddMembersRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Adding members to class {ClassId}", classId);

        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post,
                AppIds.Accessor,
                $"classes-accessor/{classId:D}/members",
                request,
                ct
            );

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Bad request while adding members to class {ClassId}", classId);
            return false;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Already exists");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add members to class {ClassId}", classId);
            throw;
        }
    }

    public async Task<bool> RemoveMembersFromClassAsync(Guid classId, RemoveMembersRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Removing members from class {ClassId}", classId);

        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                $"classes-accessor/{classId:D}/members",
                request,
                ct
            );

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Bad request while removing members from class {ClassId}", classId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove members from class {ClassId}", classId);
            throw;
        }
    }
    public async Task<bool> DeleteClassAsync(Guid classId, CancellationToken ct)
    {
        _logger.LogInformation("Deleting class {ClassId}", classId);

        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                $"classes-accessor/{classId:D}",
                ct
            );

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Bad request while deleting class {ClassId}", classId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deleting class {ClassId}", classId);
            throw;
        }
    }

    public async Task<UserGameConfig> GetUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct)
    {
        _logger.LogInformation("Get User's Game Configuration. UserId={UserId}, Game Name={GameName}", userId, gameName);

        try
        {

            var response = await _daprClient.InvokeMethodAsync<UserGameConfig>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"game-config-accessor?userId={userId}&gameName={gameName}",
                cancellationToken: ct
            );
            return response;
        }
        catch (Exception)
        {
            _logger.LogInformation("Failed to get user configuration for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task SaveUserGameConfigAsync(Guid userId, UserNewGameConfig gameName, CancellationToken ct)
    {
        _logger.LogInformation("Save User's Game Configuration. UserId={UserId}, Game Name={GameName}", userId, gameName);
        try
        {
            var payload = _mapper.Map<UserGameConfig>(gameName);
            payload.UserId = userId;

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put,
                AppIds.Accessor,
                $"game-config-accessor",
                payload,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user configuration for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task DeleteUserGameConfigAsync(Guid userId, GameName gameName, CancellationToken ct)
    {
        _logger.LogInformation("Delete User's Game Configuration. UserId={UserId}, Game Name={GameName}", userId, gameName);
        try
        {
            var payload = new UserGameConfigKey
            {
                UserId = userId,
                GameName = gameName
            };

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                $"game-config-accessor",
                payload,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user configuration for UserId={UserId}", userId);
            throw;
        }
    }

    public async Task UpdateUserLanguageAsync(Guid callerId, SupportedLanguage language, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating user language to {Language}", language);

        var payload = new UserLanguage
        {
            UserId = callerId,
            Language = language
        };

        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put,
                AppIds.Accessor,
                $"users-accessor/language",
                payload,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user language to {Language}", language);
            throw;
        }
    }
}