namespace Manager.Models.WordCards;

public class WordCard
{
    public Guid CardId { get; set; }
    public string Hebrew { get; set; } = default!;
    public string English { get; set; } = default!;
    public bool IsLearned { get; set; }
}
