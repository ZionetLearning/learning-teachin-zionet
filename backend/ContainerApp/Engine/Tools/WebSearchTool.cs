using System.ComponentModel;
using Engine.Services;

namespace Engine.Tools;

public sealed class WebSearchTool
{
    private readonly ITavilySearchService _searchService;
    private readonly ILogger<WebSearchTool> _logger;

    public WebSearchTool(ITavilySearchService searchService, ILogger<WebSearchTool> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Description("Searches the web for current information, news, facts, or any topic. Use this when the user asks about recent events, current information, or anything that requires up-to-date knowledge from the internet.")]
    public async Task<string> SearchWebAsync(
        [Description("The search query to look up on the web")] string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("WebSearchTool invoked with query: {Query}", query);

            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Empty search query provided");
                return "Please provide a valid search query.";
            }

            var result = await _searchService.SearchAsync(query, cancellationToken);

            _logger.LogInformation("WebSearchTool completed successfully for query: {Query}", query);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSearchTool for query: {Query}", query);
            return "I encountered an error while searching the web. Please try again later.";
        }
    }
}
