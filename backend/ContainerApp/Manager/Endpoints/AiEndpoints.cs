using Manager.Constants;
using Manager.Services;
using Manager.Models;

namespace Manager.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this WebApplication app)
    {
        // 1) Пользователь задаёт вопрос
        app.MapPost("/ai/question",
            async (AiRequestModel dto, IAiGatewayService aiService, ILoggerFactory logger, CancellationToken ct) =>
            {
                var log = logger.CreateLogger("AiEndpoints.Http");
                try
                {
                    // TTL можем брать из dto (если хочешь гибко) или фиксированно
                    var id = await aiService.SendQuestionAsync(dto.Question, ct);
                    log.LogInformation("Вопрос {Id} принят", id);
                    return Results.Accepted($"/ai/answer/{id}", new { questionId = id });
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Ошибка при отправке вопроса");
                    return Results.Problem("AI question failed");
                }
            });

        // 2) Проверяем ответ
        app.MapGet("/ai/answer/{id}",
            async (string id, IAiGatewayService ai, ILoggerFactory lf, CancellationToken ct) =>
            {
                var log = lf.CreateLogger("AiEndpoints.Http");
                var ans = await ai.GetAnswerAsync(id, ct);
                if (ans is null)
                {
                    log.LogDebug("Ответ для {Id} ещё не готов", id);
                    return Results.NotFound(new { error = "Answer not ready" });
                }
                return Results.Ok(new { id, answer = ans });
            });

        // 3) Dapr приносит ответ от AI
        app.MapPost($"/{QueueNames.AiToManager}-input",
            async (AiResponseModel msg, IAiGatewayService ai, ILoggerFactory lf, CancellationToken ct) =>
            {
                var log = lf.CreateLogger("AiEndpoints.PubSub");
                try
                {
                    await ai.SaveAnswerAsync(msg, ct);
                    log.LogInformation("Ответ сохранён");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Ошибка при сохранении ответа");
                    return Results.Problem("AI answer handling failed");
                }
            });
    }
}