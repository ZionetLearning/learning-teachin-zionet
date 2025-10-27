namespace Manager.Models.WordCards;

public class LearnedStatus
{
    public required Guid CardId { get; set; }
    public required bool IsLearned { get; set; }
}
