namespace Manager.Models.WordCards;

public class CreateWordCard
{
    public required Guid UserId { get; set; }
    public required string Hebrew { get; set; }
    public required string English { get; set; }
    public string? Definition { get; set; }
}
