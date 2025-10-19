namespace Accessor.Models.WordCards;

public class WordCardModel
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public string Hebrew { get; set; } = default!;
    public string English { get; set; } = default!;
    public bool IsLearned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
