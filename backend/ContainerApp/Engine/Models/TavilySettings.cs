using System.ComponentModel.DataAnnotations;

namespace Engine.Models;

public sealed class TavilySettings
{
    [Required(ErrorMessage = "Tavily API key is required")]
    public required string ApiKey { get; set; }

    public int MaxResults { get; set; } = 5;

    public bool IncludeAnswer { get; set; } = true;

    public bool IncludeRawContent { get; set; } = false;

    [RegularExpression("^(basic|advanced)$", ErrorMessage = "SearchDepth must be either 'basic' or 'advanced'")]
    public string SearchDepth { get; set; } = "basic"; // "basic" or "advanced"
}
