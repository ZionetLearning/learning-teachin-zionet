using Manager.Models.Users;

namespace Manager.Services.Clients.Accessor.Interfaces;

public interface IUsersAccessorClient
{
    Task<UserData?> GetUserAsync(Guid userId);
    Task<bool> CreateUserAsync(UserModel user);
    Task<bool> UpdateUserAsync(UpdateUserModel user, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<UserData>> GetAllUsersAsync(CancellationToken ct = default);
    Task<IEnumerable<UserData>> GetUsersForCallerAsync(CallerContextDto context, CancellationToken ct = default);
    Task<bool> AssignStudentToTeacherAsync(TeacherStudentMapDto map, CancellationToken ct = default);
    Task<bool> UnassignStudentFromTeacherAsync(TeacherStudentMapDto map, CancellationToken ct = default);
    Task<IEnumerable<UserData>> GetStudentsForTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<UserData>> GetTeachersForStudentAsync(Guid studentId, CancellationToken ct = default);
    Task UpdateUserLanguageAsync(Guid callerId, SupportedLanguage language, CancellationToken ct = default);
}
