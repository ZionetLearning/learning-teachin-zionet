namespace Manager.Models.Speech;

public record VisemeData
{
    public int VisemeId { get; set; }
    public long OffsetMs { get; set; }
}