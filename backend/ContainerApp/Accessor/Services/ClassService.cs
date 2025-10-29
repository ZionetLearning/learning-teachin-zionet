using Accessor.DB;
using Accessor.Models.Classes;
using Accessor.Models.Users;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
        _db.Class.Add(model);
        await _db.SaveChangesAsync(ct);
        return model;
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
        var cls = await _db.Class
            .Include(c => c.Memberships)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(c => c.ClassId == classId, ct);

        if (cls is null)
        {
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
                    Name = $"{m.User.FirstName} {m.User.LastName}"
                })
                .ToList()
        };
    }

    public async Task<List<Class>> GetClassesForUserAsync(Guid userId, Role role, CancellationToken ct)
        => await _db.ClassMembership
            .Where(m => m.UserId == userId && m.Role == role)
            .Select(m => m.Class)
            .ToListAsync(ct);
}
