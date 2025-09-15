using Accessor.Models.Auth;
using Accessor.Models.Users;

namespace Accessor.Services.Interfaces;

public interface IUserService
{
    Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password);
    Task<UserData?> GetUserAsync(Guid userId);
    Task<List<UserData>> GetAllUsersAsync();
    Task<bool> CreateUserAsync(UserModel newUser);
    Task<bool> UpdateUserAsync(UpdateUserModel updateUser, Guid userId);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<IEnumerable<UserData>> GetAllUsersAsync(Role? roleFilter = null, Guid? teacherId = null, CancellationToken ct = default);
    Task<IEnumerable<UserData>> GetStudentsForTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<bool> AssignStudentToTeacherAsync(Guid teacherId, Guid studentId, CancellationToken ct = default);
    Task<bool> UnassignStudentFromTeacherAsync(Guid teacherId, Guid studentId, CancellationToken ct = default);
    Task<IEnumerable<UserData>> GetTeachersForStudentAsync(Guid studentId, CancellationToken ct = default);

}
