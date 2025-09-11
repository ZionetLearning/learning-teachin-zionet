using Accessor.DB;
using Accessor.Models.Auth;
using Accessor.Models.Users;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accessor.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly AccessorDbContext _db;

    public UserService(AccessorDbContext db, ILogger<UserService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            return null;
        }

        return new AuthenticatedUser { UserId = user.UserId, Role = user.Role };
    }

    public async Task<UserData?> GetUserAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            return null;
        }

        return new UserData
        {
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            PreferredLanguageCode = user.PreferredLanguageCode,
            HebrewLevelValue = user.HebrewLevelValue
        };
    }

    public Task<List<UserData>> GetAllUsersAsync() =>
        _db.Users
            .Select(u => new UserData
            {
                UserId = u.UserId,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Role = u.Role,
                PreferredLanguageCode = u.PreferredLanguageCode,
                HebrewLevelValue = u.HebrewLevelValue
            })
            .ToListAsync();

    public async Task<bool> CreateUserAsync(UserModel newUser)
    {
        if (await _db.Users.AnyAsync(u => u.Email == newUser.Email))
        {
            return false;
        }

        await _db.Users.AddAsync(newUser);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserAsync(UpdateUserModel updateUser, Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            return false;
        }

        if (updateUser.FirstName is not null)
        {
            user.FirstName = updateUser.FirstName;
        }

        if (updateUser.LastName is not null)
        {
            user.LastName = updateUser.LastName;
        }

        if (updateUser.Email is not null)
        {
            user.Email = updateUser.Email;
        }

        if (updateUser.PreferredLanguageCode is not null)
        {
            user.PreferredLanguageCode = updateUser.PreferredLanguageCode.Value;
        }

        if (updateUser.HebrewLevelValue is not null)
        {
            user.HebrewLevelValue = updateUser.HebrewLevelValue;
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _db.Users.FindAsync(userId);

        if (user == null)
        {
            return false;
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
    public async Task<IEnumerable<UserData>> GetAllUsersAsync(Role? roleFilter = null, Guid? teacherId = null, CancellationToken ct = default)
    {
        _logger.LogInformation("GetAllUsers START (roleFilter={Role}, teacherId={Teacher})",
            roleFilter?.ToString() ?? "none", teacherId?.ToString() ?? "none");

        try
        {
            var query = _db.Users.AsNoTracking();

            if (roleFilter.HasValue)
            {
                query = query.Where(u => u.Role == roleFilter.Value);
                _logger.LogDebug("Applied role filter: {Role}", roleFilter.Value);
            }

            var users = await query
                .Select(u => new UserData
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role
                })
                .ToListAsync(ct);

            _logger.LogInformation("GetAllUsers END: returned {Count} users", users.Count);
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllUsers FAILED (roleFilter={Role}, teacherId={Teacher})",
                roleFilter?.ToString() ?? "none", teacherId?.ToString() ?? "none");
            throw;
        }
    }

    public async Task<IEnumerable<UserData>> GetStudentsForTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        _logger.LogInformation("GetStudentsForTeacher START (teacherId={TeacherId})", teacherId);

        try
        {
            var studentIds = await _db.TeacherStudents
                .AsNoTracking()
                .Where(ts => ts.TeacherId == teacherId)
                .Select(ts => ts.StudentId)
                .ToListAsync(ct);

            _logger.LogDebug("Found {Count} studentIds for teacher {TeacherId}", studentIds.Count, teacherId);

            if (studentIds.Count == 0)
            {
                _logger.LogInformation("GetStudentsForTeacher END: no students for teacher {TeacherId}", teacherId);
                return Enumerable.Empty<UserData>();
            }

            var students = await _db.Users
                .AsNoTracking()
                .Where(u => studentIds.Contains(u.UserId) && u.Role == Role.Student)
                .Select(u => new UserData
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role
                })
                .ToListAsync(ct);

            _logger.LogInformation("GetStudentsForTeacher END: returned {Count} students for teacher {TeacherId}", students.Count, teacherId);
            return students;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetStudentsForTeacher FAILED (teacherId={TeacherId})", teacherId);
            throw;
        }
    }

    public async Task<bool> AssignStudentToTeacherAsync(Guid teacherId, Guid studentId, CancellationToken ct = default)
    {
        _logger.LogInformation("AssignStudentToTeacher START (teacherId={TeacherId}, studentId={StudentId})", teacherId, studentId);

        try
        {
            var teacherOk = await _db.Users.AnyAsync(u => u.UserId == teacherId && u.Role == Role.Teacher, ct);
            if (!teacherOk)
            {
                _logger.LogWarning("AssignStudentToTeacher: teacher not found or not a Teacher (teacherId={TeacherId})", teacherId);
                return false;
            }

            var studentOk = await _db.Users.AnyAsync(u => u.UserId == studentId && u.Role == Role.Student, ct);
            if (!studentOk)
            {
                _logger.LogWarning("AssignStudentToTeacher: student not found or not a Student (studentId={StudentId})", studentId);
                return false;
            }

            _db.TeacherStudents.Add(new TeacherStudent { TeacherId = teacherId, StudentId = studentId });
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("AssignStudentToTeacher END: assignment created (teacherId={TeacherId}, studentId={StudentId})",
                teacherId, studentId);
            return true;
        }
        catch (DbUpdateException ex)
        {
            if (IsUniqueViolation(ex))
            {
                _logger.LogInformation("AssignStudentToTeacher: mapping already existed (teacherId={TeacherId}, studentId={StudentId})",
                    teacherId, studentId);
                return true;
            }

            _logger.LogError(ex, "AssignStudentToTeacher FAILED (teacherId={TeacherId}, studentId={StudentId})", teacherId, studentId);
            throw;
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        return ex.InnerException is PostgresException pg && pg.SqlState == "23505";
    }

    public async Task<bool> UnassignStudentFromTeacherAsync(Guid teacherId, Guid studentId, CancellationToken ct = default)
    {
        _logger.LogInformation("UnassignStudentFromTeacher START (teacherId={TeacherId}, studentId={StudentId})", teacherId, studentId);

        try
        {
            var rel = await _db.TeacherStudents
                .Where(ts => ts.TeacherId == teacherId && ts.StudentId == studentId)
                .FirstOrDefaultAsync(ct);

            if (rel == null)
            {
                _logger.LogInformation("UnassignStudentFromTeacher: no relation found (teacherId={TeacherId}, studentId={StudentId})",
                    teacherId, studentId);
                return true;
            }

            _db.TeacherStudents.Remove(rel);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("UnassignStudentFromTeacher END: relation removed (teacherId={TeacherId}, studentId={StudentId})",
                teacherId, studentId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UnassignStudentFromTeacher FAILED (teacherId={TeacherId}, studentId={StudentId})", teacherId, studentId);
            throw;
        }
    }

    public async Task<IEnumerable<UserData>> GetTeachersForStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        _logger.LogInformation("GetTeachersForStudent START (studentId={StudentId})", studentId);

        try
        {
            var teacherIds = await _db.TeacherStudents
                .AsNoTracking()
                .Where(ts => ts.StudentId == studentId)
                .Select(ts => ts.TeacherId)
                .ToListAsync(ct);

            _logger.LogDebug("Found {Count} teacherIds for student {StudentId}", teacherIds.Count, studentId);

            if (teacherIds.Count == 0)
            {
                _logger.LogInformation("GetTeachersForStudent END: no teachers for student {StudentId}", studentId);
                return Enumerable.Empty<UserData>();
            }

            var teachers = await _db.Users
                .AsNoTracking()
                .Where(u => teacherIds.Contains(u.UserId) && u.Role == Role.Teacher)
                .Select(u => new UserData
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Role = u.Role
                })
                .ToListAsync(ct);

            _logger.LogInformation("GetTeachersForStudent END: returned {Count} teachers for student {StudentId}", teachers.Count, studentId);
            return teachers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTeachersForStudent FAILED (studentId={StudentId})", studentId);
            throw;
        }
    }
}