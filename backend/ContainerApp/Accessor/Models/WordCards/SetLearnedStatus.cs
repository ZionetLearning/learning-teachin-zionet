namespace Accessor.Models.WordCards;

public class SetLearnedStatus
{
    public required Guid UserId { get; set; }
    public required Guid CardId { get; set; }
    public required bool IsLearned { get; set; }
}
