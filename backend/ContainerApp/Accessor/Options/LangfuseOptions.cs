namespace Accessor.Options;

public class LangfuseOptions
{
    public string? BaseUrl { get; set; }
    public string? PublicKey { get; set; }
    public string? SecretKey { get; set; }

    public bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(BaseUrl) &&
               !string.IsNullOrWhiteSpace(PublicKey) &&
               !string.IsNullOrWhiteSpace(SecretKey);
    }
}
