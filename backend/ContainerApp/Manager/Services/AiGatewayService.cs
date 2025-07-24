using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using System.Collections.Concurrent;

namespace Manager.Services
{
    public sealed class AiGatewayService : IAiGatewayService
    {
        private readonly DaprClient _dapr;
        private readonly ILogger<AiGatewayService> _log;

        // In‑memory cache → replace with a proper store later
        private static readonly ConcurrentDictionary<string, string> _answers = new();
        public AiGatewayService(DaprClient dapr, ILogger<AiGatewayService> log)
        {
            _dapr = dapr ?? throw new ArgumentNullException(nameof(dapr));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }
        public async Task<string> SendQuestionAsync(string question, CancellationToken ct = default)
        {
            var msg = AiRequestModel.Create(question, TopicNames.AiToManager);

            _log.LogInformation("Send question {Id} (TTL={Ttl})", msg.Id, msg.TtlSeconds);

            try
            {
                await _dapr.PublishEventAsync("pubsub", TopicNames.ManagerToAi, msg, ct);
                return msg.Id;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to publish question {Id} to topic {Topic}", msg.Id, TopicNames.ManagerToAi);
                throw; // let the upper layer decide what to do
            }
        }

        public Task SaveAnswerAsync(AiResponseModel msg, CancellationToken ct = default)
        {
            _log.LogInformation("Start endpoint ${Name}.", nameof(SaveAnswerAsync));

            try
            {
                _answers[msg.Id] = msg.Answer;
                _log.LogInformation("Answer saved for {CorrelationId}", msg.Id);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to cache answer for {CorrelationId}", msg.Id);
                throw; 
            }
        }

        public Task<string?> GetAnswerAsync(string questionIdOrHash, CancellationToken ct = default)
        {
            _log.LogInformation("Start endpoint ${Name}.", nameof(GetAnswerAsync));

            try
            {
                _answers.TryGetValue(questionIdOrHash, out var answer);
                return Task.FromResult(answer);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to read answer for {CorrelationId}", questionIdOrHash);
                return Task.FromResult<string?>(null);
            }
        }

    }
}