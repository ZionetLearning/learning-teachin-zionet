using System.Net;
using Dapr.Client;
using Manager.Constants;
using Manager.Models.Users;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.Classes;

namespace Manager.Services.Clients.Accessor;

public class ClassesAccessorClient : IClassesAccessorClient
{
    private readonly ILogger<ClassesAccessorClient> _logger;
    private readonly DaprClient _daprClient;

    public ClassesAccessorClient(ILogger<ClassesAccessorClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<GetClassAccessorResponse?> GetClassAsync(Guid classId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching class {ClassId} from Accessor", classId);

        try
        {
            // Accessor returns ClassDto which has the same shape we need
            var result = await _daprClient.InvokeMethodAsync<AccessorClassDto>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"classes-accessor/{classId:D}",
                ct
            );

            if (result == null)
            {
                return null;
            }

            return new GetClassAccessorResponse
            {
                ClassId = result.ClassId,
                Name = result.Name,
                Members = result.Members.Select(m => new MemberAccessorDto
                {
                    MemberId = m.MemberId,
                    Name = m.Name,
                    Role = (Role)m.Role
                }).ToList()
            };
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

    public async Task<GetMyClassesAccessorResponse?> GetMyClassesAsync(Guid userId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching classes for user {UserId} from Accessor", userId);

        try
        {
            var result = await _daprClient.InvokeMethodAsync<List<AccessorClassDto>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"classes-accessor/my/{userId:D}",
                ct
            );

            if (result == null)
            {
                return null;
            }

            return new GetMyClassesAccessorResponse
            {
                Classes = result.Select(c => new ClassAccessorDto
                {
                    ClassId = c.ClassId,
                    Name = c.Name,
                    Members = c.Members.Select(m => new MemberAccessorDto
                    {
                        MemberId = m.MemberId,
                        Name = m.Name,
                        Role = (Role)m.Role
                    }).ToList()
                }).ToList()
            };
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Classes for user {UserId} not found (404)", userId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch classes for user {UserId} from Accessor", userId);
            throw;
        }
    }

    public async Task<GetAllClassesAccessorResponse?> GetAllClassesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching all classes from Accessor");

        try
        {
            var result = await _daprClient.InvokeMethodAsync<List<AccessorClassDto>>(
                HttpMethod.Get,
                AppIds.Accessor,
                "classes-accessor/",
                ct
            );

            if (result == null)
            {
                return null;
            }

            return new GetAllClassesAccessorResponse
            {
                Classes = result.Select(c => new ClassAccessorDto
                {
                    ClassId = c.ClassId,
                    Name = c.Name,
                    Members = c.Members.Select(m => new MemberAccessorDto
                    {
                        MemberId = m.MemberId,
                        Name = m.Name,
                        Role = (Role)m.Role
                    }).ToList()
                }).ToList()
            };
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

    public async Task<CreateClassAccessorResponse?> CreateClassAsync(CreateClassAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Creating class {Name} via Accessor", request.Name);

        try
        {
            var accessorRequest = new AccessorCreateClassRequest
            {
                Name = request.Name,
                Description = request.Description
            };

            var result = await _daprClient.InvokeMethodAsync<AccessorCreateClassRequest, AccessorClassCreatedDto>(
                HttpMethod.Post,
                AppIds.Accessor,
                "classes-accessor",
                accessorRequest,
                ct
            );

            if (result == null)
            {
                return null;
            }

            return new CreateClassAccessorResponse
            {
                ClassId = result.ClassId,
                Name = result.Name,
                Code = result.Code ?? string.Empty,
                Description = result.Description,
                CreatedAt = result.CreatedAt
            };
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

    public async Task<bool> AddMembersToClassAsync(Guid classId, AddMembersAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Adding members to class {ClassId}", classId);

        try
        {
            var accessorRequest = new AccessorAddMembersRequest
            {
                UserIds = request.UserIds,
                AddedBy = request.AddedBy
            };

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Post,
                AppIds.Accessor,
                $"classes-accessor/{classId:D}/members",
                accessorRequest,
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
            _logger.LogWarning("Member already exists in class {ClassId}", classId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add members to class {ClassId}", classId);
            throw;
        }
    }

    public async Task<bool> RemoveMembersFromClassAsync(Guid classId, RemoveMembersAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Removing members from class {ClassId}", classId);

        try
        {
            var accessorRequest = new AccessorRemoveMembersRequest
            {
                UserIds = request.UserIds
            };

            await _daprClient.InvokeMethodAsync(
                HttpMethod.Delete,
                AppIds.Accessor,
                $"classes-accessor/{classId:D}/members",
                accessorRequest,
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

    public async Task<bool> DeleteClassAsync(Guid classId, CancellationToken ct = default)
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
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Class {ClassId} not found for deletion", classId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete class {ClassId}", classId);
            throw;
        }
    }

    // Internal DTOs representing what Accessor actually returns
    private sealed record AccessorClassDto
    {
        public Guid ClassId { get; init; }
        public required string Name { get; init; }
        public required List<AccessorMemberDto> Members { get; init; }
    }

    private sealed record AccessorMemberDto
    {
        public Guid MemberId { get; init; }
        public required string Name { get; init; }
        public int Role { get; init; }
    }

    private sealed record AccessorClassCreatedDto
    {
        public Guid ClassId { get; init; }
        public required string Name { get; init; }
        public string? Code { get; init; }
        public string? Description { get; init; }
        public DateTime CreatedAt { get; init; }
    }

    private sealed record AccessorCreateClassRequest
    {
        public required string Name { get; init; }
        public string? Description { get; init; }
    }

    private sealed record AccessorAddMembersRequest
    {
        public required IEnumerable<Guid> UserIds { get; init; }
        public required Guid AddedBy { get; init; }
    }

    private sealed record AccessorRemoveMembersRequest
    {
        public required IEnumerable<Guid> UserIds { get; init; }
    }
}
