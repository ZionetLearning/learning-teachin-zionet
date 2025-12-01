using System.Net;
using Dapr.Client;
using Manager.Constants;
using Manager.Services.Clients.Accessor.Interfaces;
using Manager.Services.Clients.Accessor.Models.WordCards;

namespace Manager.Services.Clients.Accessor;

public class WordCardsAccessorClient : IWordCardsAccessorClient
{
    private readonly ILogger<WordCardsAccessorClient> _logger;
    private readonly DaprClient _daprClient;

    public WordCardsAccessorClient(ILogger<WordCardsAccessorClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<GetWordCardsAccessorResponse> GetWordCardsAsync(Guid userId, DateTime? fromDate = null, DateTime? toDate = null, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Inside: {Method} in {Class}. UserId={UserId}, FromDate={FromDate}, ToDate={ToDate}",
            nameof(GetWordCardsAsync), nameof(WordCardsAccessorClient), userId, fromDate, toDate);

        try
        {
            var queryParams = new List<string>();

            if (fromDate.HasValue)
            {
                queryParams.Add($"fromDate={fromDate.Value:O}");
            }

            if (toDate.HasValue)
            {
                queryParams.Add($"toDate={toDate.Value:O}");
            }

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : string.Empty;

            var wordCards = await _daprClient.InvokeMethodAsync<List<WordCardAccessorDto>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"wordcards-accessor/{userId}{queryString}",
                ct
            );

            return new GetWordCardsAccessorResponse
            {
                WordCards = wordCards ?? new List<WordCardAccessorDto>()
            };
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No word cards found for user {UserId}", userId);
            return new GetWordCardsAccessorResponse { WordCards = Array.Empty<WordCardAccessorDto>() };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch word cards for user {UserId}", userId);
            throw;
        }
    }

    public async Task<CreateWordCardAccessorResponse> CreateWordCardAsync(CreateWordCardAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(CreateWordCardAsync), nameof(WordCardsAccessorClient));

        try
        {
            var response = await _daprClient.InvokeMethodAsync<CreateWordCardAccessorRequest, CreateWordCardAccessorResponse>(
                HttpMethod.Post,
                AppIds.Accessor,
                "wordcards-accessor",
                request,
                ct
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create word card");
            throw;
        }
    }

    public async Task<UpdateLearnedStatusAccessorResponse> UpdateLearnedStatusAsync(UpdateLearnedStatusAccessorRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}", nameof(UpdateLearnedStatusAsync), nameof(WordCardsAccessorClient));
        try
        {
            var response = await _daprClient.InvokeMethodAsync<UpdateLearnedStatusAccessorRequest, UpdateLearnedStatusAccessorResponse>(
                HttpMethod.Patch,
                AppIds.Accessor,
                "wordcards-accessor/learned",
                request,
                ct
            );

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update learned status for CardId={CardId}", request.CardId);
            throw;
        }
    }
}
