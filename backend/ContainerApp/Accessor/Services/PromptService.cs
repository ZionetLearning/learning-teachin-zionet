using System.Globalization;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Accessor.DB;
using Accessor.Models.Prompts;
using Accessor.Options;
using Microsoft.Extensions.Options;

namespace Accessor.Services;

public class PromptService : IPromptService
{
    private readonly ILogger<PromptService> _logger;
    private readonly AccessorDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IOptions<PromptsOptions> _promptsOptions;

    public PromptService(
        ILogger<PromptService> logger,
        AccessorDbContext dbContext,
        IMapper mapper,
        IOptions<PromptsOptions> promptsOptions)
    {
        _logger = logger;
        _dbContext = dbContext;
        _mapper = mapper;
        _promptsOptions = promptsOptions;
    }

    public async Task<PromptResponse> CreatePromptAsync(CreatePromptRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        using var scope = _logger.BeginScope("PromptKey: {PromptKey}", request.PromptKey);
        ValidateRequest(request);

        try
        {
            // Idempotent: if latest has identical content, reuse it
            var latest = await GetLatestPromptInternalAsync(request.PromptKey, track: false, cancellationToken: cancellationToken);
            if (latest is not null &&
                string.Equals(latest.Content, request.Content, StringComparison.Ordinal))
            {
                _logger.LogInformation("Content unchanged; returning existing prompt");
                return latest;
            }

            var model = new PromptModel
            {
                Id = Guid.NewGuid(),
                PromptKey = request.PromptKey,
                Version = GenerateVersion(),
                Content = request.Content
            };

            _dbContext.Prompts.Add(model);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created prompt internal version {Version}", model.Version);
            return _mapper.Map<PromptResponse>(model);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database failure creating prompt");
            throw new InvalidOperationException("Failed to persist prompt", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating prompt");
            throw;
        }
    }

    public async Task<List<PromptResponse>> GetAllVersionsAsync(string promptKey, CancellationToken cancellationToken = default)
    {
        ValidatePromptKey(promptKey);

        try
        {
            var list = await _dbContext.Prompts
                .AsNoTracking()
                .Where(p => p.PromptKey == promptKey)
                .OrderByDescending(p => p.Version)
                .ProjectTo<PromptResponse>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken: cancellationToken);

            _logger.LogInformation("Retrieved {Count} prompt entries for {PromptKey}", list.Count, promptKey);
            return list;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed retrieving prompts for {PromptKey}", promptKey);
            throw;
        }
    }

    public async Task<PromptResponse?> GetLatestPromptAsync(string promptKey, CancellationToken cancellationToken = default)
    {
        ValidatePromptKey(promptKey);

        try
        {
            return await GetLatestPromptInternalAsync(promptKey, track: false, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed retrieving latest prompt for {PromptKey}", promptKey);
            throw;
        }
    }

    public async Task<List<PromptResponse>> GetLatestPromptsAsync(IEnumerable<string> promptKeys, CancellationToken cancellationToken = default)
    {
        if (promptKeys is null)
        {
            throw new ArgumentNullException(nameof(promptKeys));
        }

        var keys = promptKeys
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (keys.Count == 0)
        {
            throw new ArgumentException("At least one prompt key is required.", nameof(promptKeys));
        }

        try
        {
            _logger.LogInformation("Batch retrieving {Count} prompt keys", keys.Count);

            var latestPerKey = await _dbContext.Prompts
                .AsNoTracking()
                .Where(p => keys.Contains(p.PromptKey))
                .GroupBy(p => p.PromptKey)
                .Select(g => g.OrderByDescending(p => p.Version).First())
                .ProjectTo<PromptResponse>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken: cancellationToken);

            _logger.LogInformation("Batch retrieved {Found} / {Requested} prompts", latestPerKey.Count, keys.Count);
            return latestPerKey;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed batch retrieving prompts");
            throw;
        }
    }

    public async Task InitializeDefaultPromptsAsync()
    {
        var defaults = _promptsOptions.Value.Defaults ?? new Dictionary<string, string>();

        if (defaults.Count == 0)
        {
            _logger.LogInformation("No default prompts configured; skipping initialization");
            return;
        }

        try
        {
            var keys = defaults.Keys.ToList();
            var existingKeys = await _dbContext.Prompts
                .AsNoTracking()
                .Where(p => keys.Contains(p.PromptKey))
                .Select(p => p.PromptKey)
                .Distinct()
                .ToListAsync();

            var missing = defaults.Where(kvp => !existingKeys.Contains(kvp.Key)).ToList();
            if (missing.Count == 0)
            {
                _logger.LogInformation("Default prompts already initialized");
                return;
            }

            foreach (var (key, content) in missing)
            {
                _dbContext.Prompts.Add(new PromptModel
                {
                    Id = Guid.NewGuid(),
                    PromptKey = key,
                    Version = GenerateVersion(),
                    Content = content
                });
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Initialized {Count} default prompts", missing.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed initializing default prompts");
            throw;
        }
    }

    private static string GenerateVersion() =>
        DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'", CultureInfo.InvariantCulture);

    private async Task<PromptResponse?> GetLatestPromptInternalAsync(string promptKey, bool track, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Prompts
            .Where(p => p.PromptKey == promptKey)
            .OrderByDescending(p => p.Version);

        if (!track)
        {
            query = (IOrderedQueryable<PromptModel>)query.AsNoTracking();
        }

        var entity = await query.FirstOrDefaultAsync(cancellationToken: cancellationToken);
        return entity is null ? null : _mapper.Map<PromptResponse>(entity);
    }

    private void ValidateRequest(CreatePromptRequest request)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        ValidatePromptKey(request.PromptKey);

        if (string.IsNullOrWhiteSpace(request.Content))
        {
            throw new ArgumentException("Content is required", nameof(request));
        }
    }

    private void ValidatePromptKey(string promptKey)
    {
        if (string.IsNullOrWhiteSpace(promptKey))
        {
            throw new ArgumentException("PromptKey is required", nameof(promptKey));
        }
    }
}
