namespace ToDoAccessor.Models;

public enum TodoCommandAction
{
    Create,
    Update,
    Delete
}

public record TodoCommand
{
    public TodoCommandAction Action { get; init; }
    public Todo? Todo { get; init; }
    public string? Id { get; init; }
}
