using System.ComponentModel;
using Microsoft.SemanticKernel;
using Engine.Services;
using Engine.Constants;

namespace Engine.Plugins;

public sealed class WebSearchPlugin : ISemanticKernelPlugin
{
    private readonly ITavilySearchService _searchService;
    private readonly ILogger<WebSearchPlugin> _logger;

    public WebSearchPlugin(ITavilySearchService searchService, ILogger<WebSearchPlugin> logger)
    {
        _searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [KernelFunction(PluginNames.WebSearch)]
    [Description("Searches the web for current information, news, facts, or any topic. Use this when the user asks about recent events, current information, or anything that requires up-to-date knowledge from the internet.")]
    public async Task<string> SearchWebAsync(
        [Description("The search query to look up on the web")] string query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("WebSearchPlugin invoked with query: {Query}", query);

            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Empty search query provided");
                return "Please provide a valid search query.";
            }

            var result = await _searchService.SearchAsync(query, cancellationToken);

            _logger.LogInformation("WebSearchPlugin completed successfully for query: {Query}", query);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in WebSearchPlugin for query: {Query}", query);
            return $"I encountered an error while searching the web: {ex.Message}";
        }
    }
}
