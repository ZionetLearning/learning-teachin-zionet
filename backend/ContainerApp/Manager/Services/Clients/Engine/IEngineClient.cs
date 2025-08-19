using Manager.Models;
using Manager.Models.Speech;
using Manager.Services.Clients.Engine.Models;

namespace Manager.Services.Clients.Engine;

public interface IEngineClient
{
    Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task);
    Task<EngineChatResponse> ChatAsync(EngineChatRequest request, CancellationToken cancellationToken = default);
    Task<SpeechEngineResponse?> SynthesizeAsync(SpeechRequest request, CancellationToken cancellationToken = default);
}
