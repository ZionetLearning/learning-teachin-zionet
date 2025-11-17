using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Plugins.Web.Tavily;

#pragma warning disable SKEXP0050 // Tavily is experimental

namespace Engine.Services;

public interface ITavilySearchService
{
    Task<string> SearchAsync(string query, CancellationToken ct = default);
}

public sealed class TavilySearchService : ITavilySearchService
{
    private readonly TavilyTextSearch _tavilySearch;
    private readonly ILogger<TavilySearchService> _logger;
    private readonly Models.TavilySettings _settings;

    public TavilySearchService(
        IOptions<Models.TavilySettings> settings,
        ILogger<TavilySearchService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));

        var options = new TavilyTextSearchOptions
        {
            IncludeAnswer = _settings.IncludeAnswer,
            IncludeRawContent = _settings.IncludeRawContent,
            SearchDepth = _settings.SearchDepth == "advanced"
                ? TavilySearchDepth.Advanced
                : TavilySearchDepth.Basic
        };

        _tavilySearch = new TavilyTextSearch(_settings.ApiKey, options);
    }

    public async Task<string> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Performing Tavily web search for query: {Query}", query);

            var searchOptions = new Microsoft.SemanticKernel.Data.TextSearchOptions
            {
                Top = _settings.MaxResults
            };

            var searchResults = await _tavilySearch.GetTextSearchResultsAsync(query, searchOptions, ct);

            var resultsList = new List<Microsoft.SemanticKernel.Data.TextSearchResult>();
            await foreach (var result in searchResults.Results)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                resultsList.Add(result);
            }

            if (resultsList.Count == 0)
            {
                _logger.LogWarning("No search results found for query: {Query}", query);
                return "No results found for your search query.";
            }

            var formattedResults = FormatSearchResults(resultsList);
            _logger.LogInformation("Successfully retrieved {Count} search results", resultsList.Count);

            return formattedResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing web search for query: {Query}", query);
            throw;
        }
    }

    private string FormatSearchResults(List<Microsoft.SemanticKernel.Data.TextSearchResult> results)
    {
        var formattedResults = new StringBuilder();

        formattedResults.AppendLine("Search Results:");
        formattedResults.AppendLine();

        for (var i = 0; i < results.Count; i++)
        {
            var result = results[i];
            var name = result.Name ?? $"Result {i + 1}";
            var value = result.Value ?? string.Empty;
            var link = result.Link ?? string.Empty;

            formattedResults.AppendLine($"{i + 1}. {name}");

            if (!string.IsNullOrWhiteSpace(link))
            {
                formattedResults.AppendLine($"   URL: {link}");
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                var snippet = value.Length > 300
                    ? string.Concat(value.AsSpan(0, 300), "...")
                    : value;
                formattedResults.AppendLine($"   {snippet}");
            }

            formattedResults.AppendLine();
        }

        return formattedResults.ToString();
    }
}
