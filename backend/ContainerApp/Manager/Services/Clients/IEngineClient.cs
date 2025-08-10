using Manager.Models;

namespace Manager.Services.Clients;

public interface IEngineClient
{
    Task<(bool success, string message)> ProcessTaskAsync(TaskModel task);
    Task<(bool success, string message)> ProcessTaskLongAsync(TaskModel task);
    Task<ChatResponseDto> ChatAsync(ChatRequestDto dto, CancellationToken ct = default); // NEW

}
