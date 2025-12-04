using System.Net;
using Dapr.Client;
using Manager.Constants;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Meetings;

namespace Manager.Services.Clients.Accessor;

public class MeetingAccessorClient : IMeetingAccessorClient
{
    private readonly ILogger<MeetingAccessorClient> _logger;
    private readonly DaprClient _daprClient;

    public MeetingAccessorClient(ILogger<MeetingAccessorClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<GetMeetingAccessorResponse?> GetMeetingAsync(Guid meetingId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetMeetingAsync), nameof(MeetingAccessorClient));
        try
        {
            var meeting = await _daprClient.InvokeMethodAsync<GetMeetingAccessorResponse>(
                HttpMethod.Get,
                AppIds.Accessor,
                MeetingRoutesEndpoints.ById(meetingId),
                ct);

            return meeting;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Meeting {MeetingId} not found", meetingId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get meeting {MeetingId}", meetingId);
            throw;
        }
    }

    public async Task<IReadOnlyList<GetMeetingAccessorResponse>> GetMeetingsForUserAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetMeetingsForUserAsync), nameof(MeetingAccessorClient));
        try
        {
            var meetings = await _daprClient.InvokeMethodAsync<List<GetMeetingAccessorResponse>>(
                HttpMethod.Get,
                AppIds.Accessor,
                MeetingRoutesEndpoints.ForUser(userId),
                ct);

            return meetings ?? new List<GetMeetingAccessorResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get meetings for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CreateMeetingAccessorResponse> CreateMeetingAsync(CreateMeetingAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(CreateMeetingAsync), nameof(MeetingAccessorClient));
        try
        {
            var meeting = await _daprClient.InvokeMethodAsync<CreateMeetingAccessorRequest, CreateMeetingAccessorResponse>(
                HttpMethod.Post,
                AppIds.Accessor,
                MeetingRoutesEndpoints.Base,
                request,
                ct);

            return meeting;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create meeting");
            throw;
        }
    }

    public async Task<bool> UpdateMeetingAsync(Guid meetingId, UpdateMeetingAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateMeetingAsync), nameof(MeetingAccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put,
                AppIds.Accessor,
                MeetingRoutesEndpoints.ById(meetingId),
                request,
                ct);

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for update", meetingId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update meeting {MeetingId}", meetingId);
            throw;
        }
    }

    public async Task<bool> DeleteMeetingAsync(Guid meetingId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(DeleteMeetingAsync), nameof(MeetingAccessorClient));
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                MeetingRoutesEndpoints.ById(meetingId),
                ct);

            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Meeting {MeetingId} not found for deletion", meetingId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete meeting {MeetingId}", meetingId);
            throw;
        }
    }

    public async Task<GenerateMeetingTokenAccessorResponse> GenerateTokenForMeetingAsync(Guid meetingId, Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GenerateTokenForMeetingAsync), nameof(MeetingAccessorClient));
        try
        {
            var tokenResponse = await _daprClient.InvokeMethodAsync<GenerateMeetingTokenAccessorResponse>(
                HttpMethod.Post,
                AppIds.Accessor,
                MeetingRoutesEndpoints.GenerateToken(meetingId, userId),
                ct);

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate token for meeting {MeetingId}, user {UserId}", meetingId, userId);
            throw;
        }
    }

    public async Task<CreateOrGetIdentityAccessorResponse> CreateOrGetIdentityAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(CreateOrGetIdentityAsync), nameof(MeetingAccessorClient));
        try
        {
            var identity = await _daprClient.InvokeMethodAsync<CreateOrGetIdentityAccessorResponse>(
                HttpMethod.Post,
                AppIds.Accessor,
                MeetingRoutesEndpoints.Identity(userId),
                ct);

            return identity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or get identity for user {UserId}", userId);
            throw;
        }
    }
}
