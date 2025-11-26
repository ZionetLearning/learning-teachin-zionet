namespace Accessor.Models.Classes.Responses;

/// <summary>
/// Response model for deleting a class
/// </summary>
public sealed record DeleteClassResponse
{
    public required bool Success { get; init; }
}

