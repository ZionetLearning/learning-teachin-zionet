namespace Accessor.Models.Classes;

public class Class
{
    public Guid ClassId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ClassMembership> Memberships { get; set; } = new List<ClassMembership>();
}
