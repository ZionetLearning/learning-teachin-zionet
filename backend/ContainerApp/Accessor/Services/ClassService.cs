using Accessor.DB;
using Accessor.Exceptions;
using Accessor.Models.Classes;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Accessor.Services;
public class ClassService : IClassService
{
    private readonly ILogger<ClassService> _logger;
    private readonly AccessorDbContext _db;
    public ClassService(AccessorDbContext db, ILogger<ClassService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Class> CreateClassAsync(Class model, CancellationToken ct)
    {
        try
        {
            _db.Class.Add(model);
            await _db.SaveChangesAsync(ct);
            return model;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogWarning(ex, "Failed to add class. Already exists");
            throw new ConflictException("Class with the same name or code already exists.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add class");
            throw;
        }
    }

    public async Task<bool> AddMembersAsync(Guid classId, IEnumerable<Guid> userIds, Guid addedBy, CancellationToken ct)
    {
        try
        {
            var cls = await _db.Class.FindAsync([classId], ct);
            if (cls == null)
            {
                throw new InvalidOperationException("Class not found.");
            }

            var users = await _db.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync(ct);

            var existing = await _db.ClassMembership
                .Where(m => m.ClassId == classId && userIds.Contains(m.UserId))
                .ToListAsync(ct);

            foreach (var user in users)
            {
                if (existing.Any(m => m.UserId == user.UserId && m.Role == user.Role))
                {
                    continue;
                }

                _db.ClassMembership.Add(new ClassMembership
                {
                    ClassId = classId,
                    UserId = user.UserId,
                    Role = user.Role,
                    AddedBy = addedBy
                });
            }

            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add member");
            return false;
        }
    }
    public async Task<bool> RemoveMembersAsync(Guid classId, IEnumerable<Guid> userIds, CancellationToken ct)
    {
        try
        {
            var toRemove = await _db.ClassMembership
                        .Where(m => m.ClassId == classId && userIds.Contains(m.UserId))
                        .ToListAsync(ct);

            if (toRemove.Count == 0)
            {
                return true;
            }

            _db.ClassMembership.RemoveRange(toRemove);
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove member");
            return false;
        }
    }

    public async Task<ClassDto?> GetClassWithMembersAsync(Guid classId, CancellationToken ct)
    {
        try
        {
            var cls = await _db.Class
                .Include(c => c.Memberships)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.ClassId == classId, ct);

            if (cls is null)
            {
                _logger.LogWarning("Class with ID {ClassId} not found", classId);
                return null;
            }

            return new ClassDto
            {
                ClassId = cls.ClassId,
                Name = cls.Name,
                Members = cls.Memberships
                    .Select(m => new MemberDto
                    {
                        MemberId = m.UserId,
                        Name = $"{m.User.FirstName} {m.User.LastName}",
                        Role = m.Role,
                    })
                    .ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load class with members for ID {ClassId}", classId);
            throw;
        }
    }
    public async Task<List<ClassDto>> GetAllClassesAsync(CancellationToken ct)
    {
        try
        {
            var classes = await _db.Class
                .Include(c => c.Memberships)
                .ThenInclude(m => m.User)
                .ToListAsync(ct);

            if (classes.Count == 0)
            {
                _logger.LogInformation("No classes found");
                return new List<ClassDto>();
            }

            return classes.Select(cls => new ClassDto
            {
                ClassId = cls.ClassId,
                Name = cls.Name,
                Members = cls.Memberships
                    .Select(m => new MemberDto
                    {
                        MemberId = m.UserId,
                        Name = $"{m.User.FirstName} {m.User.LastName}",
                        Role = m.Role,
                    })
                    .ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load all classes with members");
            throw;
        }
    }

    public async Task<List<ClassDto>> GetClassesForUserWithMembersAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var classes = await _db.Class
                .Include(c => c.Memberships)
                    .ThenInclude(m => m.User)
                .Where(c => c.Memberships.Any(m => m.UserId == userId))
                .ToListAsync(ct);

            if (classes.Count == 0)
            {
                _logger.LogInformation("No classes found for user {UserId}", userId);
                return new List<ClassDto>();
            }

            return classes.Select(cls => new ClassDto
            {
                ClassId = cls.ClassId,
                Name = cls.Name,
                Members = cls.Memberships
                    .Select(m => new MemberDto
                    {
                        MemberId = m.UserId,
                        Name = $"{m.User.FirstName} {m.User.LastName}",
                        Role = m.Role,
                    })
                    .ToList()
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load classes with members for user {UserId}", userId);
            throw;
        }
    }
    public async Task<bool> DeleteClassAsync(Guid classId, CancellationToken ct)
    {
        using var scope = _logger.BeginScope("ClassID: {ClassId}:", classId);
        try
        {
            var cls = await _db.Class
                .Include(c => c.Memberships)
                .FirstOrDefaultAsync(c => c.ClassId == classId, ct);

            if (cls is null)
            {
                _logger.LogWarning("Attempted to delete non-existent class with ID");
                return false;
            }

            if (cls.Memberships.Any())
            {
                _db.ClassMembership.RemoveRange(cls.Memberships);
            }

            _db.Class.Remove(cls);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully deleted class with ID");
            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error while deleting class with ID");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting class with ID");
            throw;
        }
    }
}
