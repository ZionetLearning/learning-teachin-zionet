namespace Manager.Models.WordCards.Requests;

public class UpdateLearnedStatusRequest
{
    public required Guid CardId { get; set; }
    public required bool IsLearned { get; set; }
}
