namespace Manager.Models.WordCards.Requests;

public class CreateWordCardRequest
{
    public required string Hebrew { get; set; }
    public required string English { get; set; }
    public string? Explanation { get; set; }
}
