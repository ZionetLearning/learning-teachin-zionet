using Accessor.DB;
using Accessor.Models.WordCards;
using Accessor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Accessor.Services;

public class WordCardService : IWordCardService
{
    private readonly ILogger<WordCardService> _logger;
    private readonly AccessorDbContext _db;

    public WordCardService(AccessorDbContext db, ILogger<WordCardService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WordCard>> GetWordCardsAsync(Guid userId, DateTime? fromDate, DateTime? toDate, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Fetching word cards for user {UserId}, FromDate={FromDate}, ToDate={ToDate}",
                userId, fromDate, toDate);

            var query = _db.WordCards
                .Where(card => card.UserId == userId);

            if (fromDate.HasValue)
            {
                query = query.Where(card => card.CreatedAt >= fromDate.Value || card.UpdatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(card => card.CreatedAt <= toDate.Value || card.UpdatedAt <= toDate.Value);
            }

            var entities = await query
                .OrderByDescending(card => card.CreatedAt)
                .ToListAsync(ct);

            return entities.Select(card => new WordCard
            {
                CardId = card.CardId,
                Hebrew = card.Hebrew,
                English = card.English,
                IsLearned = card.IsLearned,
                Explanation = card.Explanation,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching word cards for user {UserId}", userId);
            throw;
        }
    }

    public async Task<WordCard> CreateWordCardAsync(CreateWordCard request, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Creating new word card for user {UserId}", request.UserId);

            var now = DateTime.UtcNow;

            var newCard = new WordCardModel
            {
                CardId = Guid.NewGuid(),
                UserId = request.UserId,
                Hebrew = request.Hebrew.Trim(),
                English = request.English.Trim(),
                IsLearned = false,
                CreatedAt = now,
                UpdatedAt = now,
                Explanation = request.Explanation,
            };

            _db.WordCards.Add(newCard);
            await _db.SaveChangesAsync(ct);

            return new WordCard
            {
                CardId = newCard.CardId,
                Hebrew = newCard.Hebrew,
                English = newCard.English,
                IsLearned = newCard.IsLearned,
                Explanation = newCard.Explanation,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating word card for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task<WordCardLearnedStatus> UpdateLearnedStatusAsync(Guid userId, Guid cardId, bool isLearned, CancellationToken ct)
    {
        try
        {
            _logger.LogInformation("Updating learned status for card {CardId} (user {UserId}) to {IsLearned}", cardId, userId, isLearned);

            var card = await _db.WordCards.FirstOrDefaultAsync(
            x => x.CardId == cardId && x.UserId == userId,
            ct
            );

            if (card is null)
            {
                _logger.LogWarning("Card {CardId} not found for user {UserId}", cardId, userId);
                throw new KeyNotFoundException($"Card with ID {cardId} not found for user {userId}.");
            }

            card.IsLearned = isLearned;
            card.UpdatedAt = DateTime.UtcNow;

            _db.WordCards.Update(card);
            await _db.SaveChangesAsync(ct);

            return new WordCardLearnedStatus
            {
                CardId = card.CardId,
                IsLearned = card.IsLearned
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating learned status for card {CardId} (user {UserId})", cardId, userId);
            throw;
        }
    }
}
