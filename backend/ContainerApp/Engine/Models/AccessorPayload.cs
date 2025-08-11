
namespace Engine.Models;
public record AccessorPayload
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public string Payload { get; set; } = string.Empty;

    public required string ActionName { get; set; }
}