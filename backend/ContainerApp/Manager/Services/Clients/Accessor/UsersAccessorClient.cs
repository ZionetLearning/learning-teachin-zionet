using System.Net;
using Dapr.Client;
using Manager.Constants;
using Manager.Services.Clients.Accessor.Models.Users;
using Manager.Services.Clients.Accessor.Interfaces;

namespace Manager.Services.Clients.Accessor;

public class UsersAccessorClient(
    ILogger<UsersAccessorClient> logger,
    DaprClient daprClient
    ) : IUsersAccessorClient
{
    private readonly ILogger<UsersAccessorClient> _logger = logger;
    private readonly DaprClient _daprClient = daprClient;

    public async Task<GetUserAccessorResponse?> GetUserAsync(Guid userId)
    {
        try
        {
            return await _daprClient.InvokeMethodAsync<GetUserAccessorResponse?>(
                HttpMethod.Get, AppIds.Accessor, $"users-accessor/{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> CreateUserAsync(CreateUserAccessorRequest user)
    {
        try
        {
            _logger.LogInformation("Creating user with email: {Email}", user.Email);

            await _daprClient.InvokeMethodAsync(HttpMethod.Post, AppIds.Accessor, "users-accessor", user);

            _logger.LogInformation("User {Email} created successfully", user.Email);
            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Conflict: User already exists: {Email}", user.Email);
            return false;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.BadRequest)
        {
            _logger.LogWarning("Bad request when creating user: {Email}", user.Email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Email}", user.Email);
            throw;
        }
    }

    public async Task<bool> UpdateUserAsync(UpdateUserAccessorRequest user, Guid userId)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Put, AppIds.Accessor, $"users-accessor/{userId}", user);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(HttpMethod.Delete, AppIds.Accessor, $"users-accessor/{userId}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<GetUserAccessorResponse>> GetAllUsersAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(GetAllUsersAsync), nameof(UsersAccessorClient));

        try
        {
            var users = await _daprClient.InvokeMethodAsync<List<GetUserAccessorResponse>>(
                HttpMethod.Get, AppIds.Accessor, "users-accessor", ct);

            _logger.LogInformation("Retrieved {Count} users from accessor", users?.Count ?? 0);
            return users ?? Enumerable.Empty<GetUserAccessorResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users from accessor");
            throw;
        }
    }

    public async Task<IEnumerable<GetUserAccessorResponse>> GetUsersForCallerAsync(GetUsersForCallerAccessorRequest context, CancellationToken ct = default)
    {
        _logger.LogInformation("GetUsersForCallerAsync(role={Role}, id={Id})", context?.CallerRole, context?.CallerId);

        try
        {
            // keep same GET+query behavior; just source values from DTO
            var roleQP = Uri.EscapeDataString(context?.CallerRole ?? string.Empty);
            var callerId = context?.CallerId ?? Guid.Empty;
            var path = $"users-accessor?callerRole={roleQP}&callerId={callerId:D}";

            var users = await _daprClient.InvokeMethodAsync<List<GetUserAccessorResponse>>(
                HttpMethod.Get,
                AppIds.Accessor,
                path,
                ct);

            _logger.LogInformation("Accessor returned {Count} users for {Role}", users?.Count ?? 0, context?.CallerRole);
            return users ?? Enumerable.Empty<GetUserAccessorResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Accessor call failed for role={Role}", context?.CallerRole);
            throw;
        }
    }

    public async Task<bool> AssignStudentToTeacherAsync(AssignStudentAccessorRequest map, CancellationToken ct = default)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post,
                AppIds.Accessor,
                $"users-accessor/teacher/{map.TeacherId:D}/students/{map.StudentId:D}",
                ct);
            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogInformation("Mapping already exists for Teacher={TeacherId}, Student={StudentId}", map.TeacherId, map.StudentId);
            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.BadRequest || ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Assign failed: {Status}", ex.Response?.StatusCode);
            return false;
        }
    }

    public async Task<bool> UnassignStudentFromTeacherAsync(UnassignStudentAccessorRequest map, CancellationToken ct = default)
    {
        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                $"users-accessor/teacher/{map.TeacherId:D}/students/{map.StudentId:D}",
                ct);
            return true;
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Mapping not found (already removed) for Teacher={TeacherId}, Student={StudentId}", map.TeacherId, map.StudentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unassign failed");
            return false;
        }
    }

    public async Task<IEnumerable<GetUserAccessorResponse>> GetStudentsForTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        try
        {
            var list = await _daprClient.InvokeMethodAsync<List<GetUserAccessorResponse>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"users-accessor/teacher/{teacherId:D}/students",
                ct);
            return list ?? Enumerable.Empty<GetUserAccessorResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStudentsForTeacher failed");
            throw;
        }
    }

    public async Task<IEnumerable<GetUserAccessorResponse>> GetTeachersForStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        try
        {
            var list = await _daprClient.InvokeMethodAsync<List<GetUserAccessorResponse>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"users-accessor/student/{studentId:D}/teachers",
                ct);
            return list ?? Enumerable.Empty<GetUserAccessorResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTeachersForStudent failed");
            throw;
        }
    }

    public async Task UpdateUserLanguageAsync(UpdateUserLanguageAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Updating user language to {Language}", request.Language);

        try
        {
            await _daprClient.InvokeMethodAsync(
                HttpMethod.Put,
                AppIds.Accessor,
                $"users-accessor/language",
                request,
                cancellationToken: ct
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user language to {Language}", request.Language);
            throw;
        }
    }
}
