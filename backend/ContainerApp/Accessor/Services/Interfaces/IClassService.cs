using Accessor.Models.Classes;
using Accessor.Models.Users;

namespace Accessor.Services.Interfaces;

public interface IClassService
{
    Task<Class> CreateClassAsync(Class model, CancellationToken ct);
    Task<bool> AddMembersAsync(Guid classId, IEnumerable<Guid> userIds, Guid addedBy, CancellationToken ct);
    Task<bool> RemoveMembersAsync(Guid classId, IEnumerable<Guid> userIds, CancellationToken ct);
    Task<Class?> GetClassWithMembersAsync(Guid classId, CancellationToken ct);
    Task<List<Class>> GetClassesForUserAsync(Guid userId, Role role, CancellationToken ct);
}
