using Accessor.Models.Auth;
using Accessor.Models.Users;
using Accessor.DB;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        if (updateUser.Role is not null)
        {
            user.Role = updateUser.Role.Value;
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
}