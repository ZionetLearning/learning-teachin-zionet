namespace Manager.Models.WordCards.Responses;

public class GetWordCardsResponse
{
    public Guid CardId { get; set; }
    public string Hebrew { get; set; } = default!;
    public string English { get; set; } = default!;
    public bool IsLearned { get; set; }
    public string? Explanation { get; set; }
}
