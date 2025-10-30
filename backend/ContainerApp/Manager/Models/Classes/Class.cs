namespace Manager.Models.Classes;

public class Class
{
    public Guid ClassId { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public record ClassDto
{
    public Guid ClassId { get; set; }
    public required string Name { get; set; }
    public required List<MemberDto> Members { get; set; }
}