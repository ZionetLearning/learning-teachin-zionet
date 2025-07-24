namespace Engine.Models
{
    public sealed class AzureOpenAiSettings
    {
        public required string Endpoint { get; init; }
        public required string ApiKey { get; init; }
        public required string DeploymentName { get; init; }
        public string? Model { get; init; }

    }
}
