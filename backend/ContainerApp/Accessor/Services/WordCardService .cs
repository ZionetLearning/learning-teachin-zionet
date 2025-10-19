using Accessor.DB;
using Accessor.Models.WordCards;
using Accessor.Services.Interfaces;

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

    public async Task<IReadOnlyList<WordCard>> GetWordCardsAsync(Guid userId, CancellationToken ct)
    {
        var entities = await _db.GetByUserIdAsync(userId, ct);

        return entities.Select(card => new WordCardResponse
        {
            CardId = card.CardId,
            Hebrew = card.Hebrew,
            English = card.English,
            IsLearned = card.IsLearned
        }).ToList();
    }

    public async Task<WordCard> CreateWordCardAsync(CreateWordCard request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var newCard = new WordCard
        {
            CardId = Guid.NewGuid(),
            UserId = request.UserId,
            Hebrew = request.Hebrew.Trim(),
            English = request.English.Trim(),
            IsLearned = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.InsertAsync(newCard, ct);

        return new WordCardResponse
        {
            CardId = newCard.CardId,
            Hebrew = newCard.Hebrew,
            English = newCard.English,
            IsLearned = newCard.IsLearned
        };
    }

    public async Task<WordCardLearnedStatus> UpdateLearnedStatusAsync(Guid userId, Guid cardId, bool isLearned, CancellationToken ct)
    {
        var card = await _repository.GetByIdAsync(cardId, ct);

        if (card == null || card.UserId != userId)
        {
            throw new UnauthorizedAccessException("Card not found or user unauthorized.");
        }

        card.IsLearned = isLearned;
        card.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(card, ct);

        return new WordCardLearnedUpdateResult
        {
            CardId = card.CardId,
            IsLearned = card.IsLearned
        };
    }
}
