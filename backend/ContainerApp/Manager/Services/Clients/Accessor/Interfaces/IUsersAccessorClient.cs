using Manager.Services.Clients.Accessor.Models.Users;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IUsersAccessorClient
{
    Task<GetUserAccessorResponse?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(CreateUserAccessorRequest user);
    Task<bool> UpdateUserAsync(UpdateUserAccessorRequest user, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<GetUserAccessorResponse>> GetAllUsersAsync(CancellationToken ct = default);
    Task<IEnumerable<GetUserAccessorResponse>> GetUsersForCallerAsync(GetUsersForCallerAccessorRequest context, CancellationToken ct = default);
    Task<bool> AssignStudentToTeacherAsync(AssignStudentAccessorRequest map, CancellationToken ct = default);
    Task<bool> UnassignStudentFromTeacherAsync(UnassignStudentAccessorRequest map, CancellationToken ct = default);
    Task<IEnumerable<GetUserAccessorResponse>> GetStudentsForTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<GetUserAccessorResponse>> GetTeachersForStudentAsync(Guid studentId, CancellationToken ct = default);
    Task UpdateUserLanguageAsync(UpdateUserLanguageAccessorRequest request, CancellationToken ct = default);
}
