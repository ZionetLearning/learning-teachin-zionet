namespace Accessor.Models.WordCards;

public class WordCardModel
{
    public Guid CardId { get; set; }
    public Guid UserId { get; set; }
    public required string Hebrew { get; set; }
    public required string English { get; set; }
    public bool IsLearned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Definition { get; set; }
}
