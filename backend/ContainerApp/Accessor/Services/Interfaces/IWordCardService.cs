using Accessor.Models.WordCards;

namespace Accessor.Services.Interfaces;

public interface IWordCardService
{
    Task<IReadOnlyList<WordCard>> GetWordCardsAsync(Guid userId, DateTime? fromDate, DateTime? toDate, CancellationToken ct);
    Task<WordCard> CreateWordCardAsync(CreateWordCard request, CancellationToken ct);
    Task<WordCardLearnedStatus> UpdateLearnedStatusAsync(Guid userId, Guid cardId, bool isLearned, CancellationToken ct);
}
