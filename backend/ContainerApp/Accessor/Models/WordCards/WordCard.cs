namespace Accessor.Models.WordCards;

public class WordCard
{
    public Guid CardId { get; set; }
    public string Hebrew { get; set; } = default!;
    public string English { get; set; } = default!;
    public bool IsLearned { get; set; }
    public string? Explanation { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
