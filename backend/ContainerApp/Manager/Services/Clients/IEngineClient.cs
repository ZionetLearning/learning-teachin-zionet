using Manager.Models;
using Manager.Models.Speech;

namespace Manager.Services.Clients;

public interface IEngineClient
{
    Task<(bool success, string message)> ProcessTaskAsync(TaskModel task, IDictionary<string, string>? metadata = null);
    Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task);
    Task<ChatResponseDto> ChatAsync(ChatRequestDto dto, CancellationToken ct = default); // NEW
    Task<SpeechEngineResponse?> SynthesizeAsync(SpeechRequest request, CancellationToken cancellationToken = default);
}
