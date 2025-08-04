namespace Accessor.Models
{
    public record UpdateTaskName
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
