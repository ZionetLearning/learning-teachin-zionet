namespace Accessor.Models.WordCards;

public class CreateWordCard
{
    public Guid UserId { get; set; }
    public required string Hebrew { get; set; }
    public required string English { get; set; }
}
