using Accessor.Models.Games;
using Accessor.Services.Interfaces;

namespace Accessor.Services;

public class StudentPracticeHistoryService : IStudentPracticeHistoryService
{
    private readonly IGameService _gameService;
    private readonly IUserService _userService;
    private readonly ILogger<StudentPracticeHistoryService> _logger;

    public StudentPracticeHistoryService(IGameService gameService, IUserService userService, ILogger<StudentPracticeHistoryService> logger)
    {
        _gameService = gameService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<PagedResult<SummaryHistoryWithStudentDto>> GetHistoryAsync(int page, int pageSize, CancellationToken ct)
    {
        _logger.LogInformation("Fetching all histories from the orchestrator. Page={Page}, PageSize={PageSize}", page, pageSize);

        try
        {
            var result = await _gameService.GetAllHistoriesAsync(page, pageSize, ct);

            var studentIds = result.Items.Select(x => x.StudentId).Distinct().ToList();
            if (!studentIds.Any())
            {
                _logger.LogWarning("No student IDs found in game history.");
                return result;
            }

            var nameMap = await _userService.GetUserFullNamesAsync(studentIds, ct);

            // Create new items with student names populated (records are immutable)
            var enrichedItems = result.Items.Select(item =>
            {
                if (nameMap.TryGetValue(item.StudentId, out var userName))
                {
                    return item with
                    {
                        StudentFirstName = userName.FirstName,
                        StudentLastName = userName.LastName
                    };
                }

                return item;
            }).ToList();

            return new PagedResult<SummaryHistoryWithStudentDto>
            {
                Items = enrichedItems,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed in orchestrator while enriching student practice history.");
            throw;
        }
    }
}
