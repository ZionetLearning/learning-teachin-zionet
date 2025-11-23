namespace Manager.Models.WordCards.Responses;

public class UpdateLearnedStatusResponse
{
    public Guid CardId { get; set; }
    public bool IsLearned { get; set; }
}
