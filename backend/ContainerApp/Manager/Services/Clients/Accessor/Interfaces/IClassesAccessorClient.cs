using Manager.Services.Clients.Accessor.Models.Classes;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IClassesAccessorClient
{
    Task<GetClassAccessorResponse?> GetClassAsync(Guid classId, CancellationToken ct = default);
    Task<GetMyClassesAccessorResponse?> GetMyClassesAsync(Guid userId, CancellationToken ct = default);
    Task<GetAllClassesAccessorResponse?> GetAllClassesAsync(CancellationToken ct = default);
    Task<CreateClassAccessorResponse?> CreateClassAsync(CreateClassAccessorRequest request, CancellationToken ct = default);
    Task<bool> AddMembersToClassAsync(Guid classId, AddMembersAccessorRequest request, CancellationToken ct = default);
    Task<bool> RemoveMembersFromClassAsync(Guid classId, RemoveMembersAccessorRequest request, CancellationToken ct = default);
    Task<bool> DeleteClassAsync(Guid classId, CancellationToken ct = default);
}
