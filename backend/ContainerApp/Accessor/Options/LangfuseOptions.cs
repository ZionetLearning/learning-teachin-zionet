namespace Accessor.Options;

public class LangfuseOptions
{
    public string BaseUrl { get; set; } = "https://teachin.westeurope.cloudapp.azure.com/langfuse";
    public required string PublicKey { get; set; }
    public required string SecretKey { get; set; }
}
