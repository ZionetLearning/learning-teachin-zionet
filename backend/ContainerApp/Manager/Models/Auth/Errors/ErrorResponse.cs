namespace Manager.Models.Auth.Errors;

public class ErrorResponse
{
    public int Status { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
