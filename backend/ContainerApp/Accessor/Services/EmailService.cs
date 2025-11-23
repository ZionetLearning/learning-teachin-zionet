using Accessor.DB;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly AccessorDbContext _db;

    public EmailService(AccessorDbContext db, ILogger<EmailService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetRecipientEmailsByNameAsync(string name, CancellationToken ct = default)
    {
        _logger.LogInformation("GetRecipientEmailsByNameAsync START (name={Name})", name);

        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("GetRecipientEmailsByNameAsync called with empty name");
            return [];
        }

        try
        {
            var exactName = name.Trim();

            var emails = await _db.Users
                .AsNoTracking()
                .Where(u =>
                    EF.Functions.ILike(u.FirstName, exactName) ||
                    EF.Functions.ILike(u.LastName, exactName) ||
                    EF.Functions.ILike(u.FirstName + " " + u.LastName, exactName))
                .Select(u => u.Email)
                .Distinct()
                .ToListAsync(ct);

            _logger.LogInformation("GetRecipientEmailsByNameAsync END: found {Count} emails for exact name={Name}", emails.Count, name);
            return emails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRecipientEmailsByNameAsync FAILED (name={Name})", name);
            throw;
        }
    }
}

