using Accessor.Models.Classes;

namespace Accessor.Services.Interfaces;

public interface IClassService
{
    Task<Class> CreateClassAsync(Class model, CancellationToken ct);
    Task<bool> AddMembersAsync(Guid classId, IEnumerable<Guid> userIds, Guid addedBy, CancellationToken ct);
    Task<bool> RemoveMembersAsync(Guid classId, IEnumerable<Guid> userIds, CancellationToken ct);
    Task<ClassDto?> GetClassWithMembersAsync(Guid classId, CancellationToken ct);
    Task<List<ClassDto>> GetAllClassesAsync(CancellationToken ct);
    Task<List<ClassDto>> GetClassesForUserWithMembersAsync(Guid userId, CancellationToken ct);
    Task<bool> DeleteClassAsync(Guid classId, CancellationToken ct);
}
