namespace TodoManager.Models
{
    public record ClientResponse
    {
        public string Message { get; init; } = string.Empty;
        public Todo Todo { get; init; } = new();
    }

}
