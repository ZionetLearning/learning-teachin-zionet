using Dapr.Client;
using Manager.Constants;
using Manager.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Manager.Services
{
    public sealed class AiGatewayService : IAiGatewayService
    {
        private readonly DaprClient _dapr;
        private readonly ILogger<AiGatewayService> _log;

        private static readonly ConcurrentDictionary<string, string> _answers = new(); //to do on real

        public AiGatewayService(DaprClient dapr, ILogger<AiGatewayService> log)
        {
            _dapr = dapr;
            _log = log;
        }
        public async Task<string> SendQuestionAsync(string question, CancellationToken ct = default)
        {
            var msg = AiRequestModel.Create(question, QueueNames.AiToManager);

            _log.LogInformation("Send question {Id} (TTL={Ttl})", msg.Id, msg.TtlSeconds);

            await _dapr.PublishEventAsync("pubsub", QueueNames.ManagerToAi, msg, ct);

            return msg.Id;
        }

        public Task SaveAnswerAsync(AiResponseModel msg, CancellationToken ct = default)
        {

            _answers[msg.Id] = msg.Answer;
            _log.LogInformation("Answer saved for {CorrelationId}", msg.Id);
            return Task.CompletedTask;
        }

        public Task<string?> GetAnswerAsync(string questionIdOrHash, CancellationToken ct = default)
        {
            _answers.TryGetValue(questionIdOrHash, out var ans);
            return Task.FromResult(ans);
        }

    }
}