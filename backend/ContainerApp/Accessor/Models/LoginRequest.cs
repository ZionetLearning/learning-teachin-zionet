using System.ComponentModel.DataAnnotations;

namespace Accessor.Models;

public class LoginRequest
{
    [EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    [StringLength(128, MinimumLength = 8)]
    public required string Password { get; init; }
}
