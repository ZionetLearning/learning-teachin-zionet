using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Manager.Services;

public sealed class AiGatewayService : IAiGatewayService
{
    private readonly DaprClient _dapr;
    private readonly ILogger<AiGatewayService> _log;
    private readonly AiSettings _settings;

    // In‑memory cache → replace with a proper store later
    private static readonly ConcurrentDictionary<string, string> Answers = new();
    public AiGatewayService(DaprClient dapr, ILogger<AiGatewayService> log, IOptions<AiSettings> options)
    {
        this._dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
        this._log = log ?? throw new ArgumentNullException(nameof(log));
        this._settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }
    public async Task<string> SendQuestionAsync(string threadId, string question, CancellationToken ct = default)
    {
        var msg = AiRequestModel.Create(question, threadId, TopicNames.AiToManager, ttlSeconds: this._settings.DefaultTtlSeconds);

        this._log.LogInformation("Send question {Id} (TTL={Ttl})", msg.Id, msg.TtlSeconds);

        try
        {
            await this._dapr.PublishEventAsync("pubsub", TopicNames.ManagerToAi, msg, ct);
            return msg.Id;
        }
        catch (Exception ex)
        {
            this._log.LogError(ex, "Failed to publish question {Id} to topic {Topic}", msg.Id, TopicNames.ManagerToAi);
            throw; // let the upper layer decide what to do
        }
    }

    public Task SaveAnswerAsync(AiResponseModel response, CancellationToken ct = default)
    {
        this._log.LogInformation("Start endpoint {Name} for threadId: {ThreadId} .", nameof(SaveAnswerAsync), response.ThreadId);

        try
        {
            Answers[response.Id] = response.Answer;
            this._log.LogInformation("Answer saved for {CorrelationId}", response.Id);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            this._log.LogError(ex, "Failed to cache answer for {CorrelationId}, for threadId: {ThreadId}", response.Id, response.ThreadId);
            throw;
        }
    }

    public Task<string?> GetAnswerAsync(string id, CancellationToken ct = default)
    {
        this._log.LogInformation("Start endpoint {Name}.", nameof(GetAnswerAsync));

        try
        {
            Answers.TryGetValue(id, out var answer);
            return Task.FromResult(answer);
        }
        catch (Exception ex)
        {
            this._log.LogError(ex, "Failed to read answer for {CorrelationId}", id);
            return Task.FromResult<string?>(null);
        }
    }
}