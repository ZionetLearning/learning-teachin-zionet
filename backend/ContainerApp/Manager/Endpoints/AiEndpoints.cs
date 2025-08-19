using Manager.Models;
using Manager.Models.Speech;
using Manager.Models.ModelValidation;
using Manager.Services;
using Microsoft.AspNetCore.Mvc;
using Manager.Models.Chat;
using Manager.Services.Clients.Engine;
using Manager.Services.Clients.Engine.Models;

namespace Manager.Endpoints;

public static class AiEndpoints
{
    private sealed class QuestionEndpoint { }
    private sealed class AnswerEndpoint { }
    private sealed class PubSubEndpoint { }
    private sealed class ChatPostEndpoint { }
    private sealed class SpeechEndpoints { }

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {

        #region HTTP GET

        app.MapGet("/ai/answer/{id}", AnswerAsync).WithName("Answer");

        #endregion

        #region HTTP POST

        app.MapPost("/ai/question", QuestionAsync).WithName("Question");

        app.MapPost("/chat", ChatAsync).WithName("Chat");
        app.MapPost("/speech/synthesize", SynthesizeAsync).WithName("SynthesizeText");

        #endregion

        return app;
    }

    private static async Task<IResult> AnswerAsync(
        [FromRoute] string id,
        [FromServices] IAiGatewayService aiService,
        [FromServices] ILogger<AnswerEndpoint> log,
        CancellationToken ct)
    {
        using (log.BeginScope("Method: {Method}, QuestionId: {Id}", nameof(AnswerAsync), id))
        {
            try
            {
                var ans = await aiService.GetAnswerAsync(id, ct);
                if (ans is null)
                {
                    log.LogInformation("Answer not ready");
                    return Results.NotFound(new { error = "Answer not ready" });
                }

                log.LogInformation("Answer returned");
                return Results.Ok(new { id, answer = ans });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to retrieve answer");
                return Results.Problem("AI answer retrieval failed");
            }
        }
    }

    private static async Task<IResult> QuestionAsync(
        [FromBody] AiRequestModel dto,
        [FromServices] IAiGatewayService aiService,
        [FromServices] ILogger<QuestionEndpoint> log,
        CancellationToken ct)
    {
        using (log.BeginScope("Method: {Method}, RequestId: {RequestId}, ThreadId: {ThreadId}",
        nameof(QuestionAsync), dto.Id, dto.ThreadId))
        {
            if (!ValidationExtensions.TryValidate(dto, out var validationErrors))
            {
                log.LogWarning("Validation failed for {Model}: {Errors}", nameof(AiRequestModel), validationErrors);
                return Results.BadRequest(new { errors = validationErrors });
            }

            try
            {
                var threadId = dto.ThreadId;

                var id = await aiService.SendQuestionAsync(threadId, dto.Question, ct);
                log.LogInformation("Request {Id} (thread {Thread}) accept", id, threadId);
                return Results.Accepted($"/ai/answer/{id}", new { questionId = id, threadId });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error sending question");
                return Results.Problem("AI question failed");
            }
        }
    }

    private static async Task<IResult> ChatAsync(
      [FromBody] ChatRequest request,
      [FromServices] IEngineClient engine,
      [FromServices] ILogger<ChatPostEndpoint> log,
      CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            return Results.BadRequest(new { error = "userMessage is required" });
        }

        var requestId = Guid.NewGuid().ToString("N");
        //todo: takeUserId from token.
        const string userId = "dev-user-001";

        if (!TryResolveThreadId(request.ThreadId, out var threadId, out var error))
        {
            return Results.BadRequest(new { error = "If threadId is present, it must be a GUID" });

        }

        var engineRequest = new EngineChatRequest
        {
            RequestId = requestId,
            ThreadId = threadId,
            UserId = userId,
            UserMessage = request.UserMessage.Trim(),
            ChatType = request.ChatType
        };

        try
        {
            var engineResponse = await engine.ChatAsync(engineRequest, ct);

            return Results.Ok(engineResponse);
        }
        catch (Dapr.Client.InvocationException ex) when (ex.Response?.StatusCode is not null)
        {
            var upstream = (int)ex.Response!.StatusCode!;
            log.LogError(ex, "Engine call failed with upstream status {Status}", upstream);

            return Results.Problem(
                title: "Engine call failed",
                detail: "Upstream service returned an error.",
                statusCode: StatusCodes.Status502BadGateway,
                extensions: new Dictionary<string, object?>
                {
                    ["upstreamStatus"] = upstream,
                    ["requestId"] = engineRequest.RequestId
                });
        }
        catch (HttpRequestException ex)
        {
            log.LogError(ex, "Engine is unreachable");
            return Results.Problem(
                title: "Engine unavailable",
                detail: "Unable to connect to the upstream service.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            log.LogWarning(ex, "Engine call timed out");
            return Results.Problem(
                title: "Engine timeout",
                detail: "Upstream service did not respond in time.",
                statusCode: StatusCodes.Status504GatewayTimeout);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return Results.StatusCode(499);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Unhandled error in Manager");
            return Results.Problem(
                title: "Unexpected server error",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static bool TryResolveThreadId(
        string? rawThreadId,
        out Guid threadId,
        out string? error)
    {
        if (!string.IsNullOrWhiteSpace(rawThreadId))
        {
            if (Guid.TryParse(rawThreadId, out threadId))
            {
                error = null;
                return true;
            }

            threadId = default;
            error = "threadId must be a valid GUID (UUID).";
            return false;
        }

        threadId = Guid.NewGuid();
        error = null;
        return true;
    }

    private static async Task<IResult> SynthesizeAsync(
       [FromBody] SpeechRequest dto,
       [FromServices] IEngineClient engineClient,
       [FromServices] ILogger<SpeechEndpoints> logger,
       CancellationToken ct)
    {
        if (dto is null || string.IsNullOrWhiteSpace(dto.Text))
        {
            return Results.BadRequest(new { error = "Text is required" });
        }

        logger.LogInformation("Received speech synthesis request for text length: {Length}", dto.Text.Length);

        try
        {
            var engineResult = await engineClient.SynthesizeAsync(dto, ct);

            if (engineResult != null)
            {
                // Transform engine response to match frontend expectations
                var response = new SpeechResponse
                {
                    AudioData = engineResult.AudioData,
                    Visemes = engineResult.Visemes,
                    Metadata = engineResult.Metadata,
                };

                logger.LogInformation("Speech synthesis completed successfully");
                return Results.Ok(response);
            }
            else
            {
                logger.LogError("Engine synthesis failed - service returned null");
                return Results.Problem("Speech synthesis failed.");
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Speech synthesis operation was canceled by user");
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Speech synthesis operation timed out");
            return Results.Problem("Speech is too long.", statusCode: StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in speech synthesis manager");
            return Results.Problem("An error occurred during speech synthesis.");
        }
    }
}