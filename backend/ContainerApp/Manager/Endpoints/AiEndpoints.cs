using Manager.Models.Chat;
using Manager.Models.Sentences;
using Manager.Models.Speech;
using Manager.Services.Clients.Accessor;
using Manager.Services.Clients.Engine;
using Manager.Services.Clients.Engine.Models;
using Microsoft.AspNetCore.Mvc;

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
        var aiGroup = app.MapGroup("/ai-manager").WithTags("AI").RequireAuthorization();

        #region HTTP GET

        // GET /ai-manager/chats/{userId}
        aiGroup.MapGet("/chats/{userId:guid}", GetChatsAsync).WithName("GetChats");
        // GET /ai-manager/chat/{chatId:guid}/{userId:guid}
        aiGroup.MapGet("/chat/{chatId:guid}/{userId:guid}", GetChatHistoryAsync).WithName("GetChatHistory");

        #endregion

        #region HTTP POST

        // POST /ai-manager/chat
        aiGroup.MapPost("/chat", ChatAsync).WithName("Chat");

        // POST /ai-manager/speech/synthesize
        aiGroup.MapPost("/speech/synthesize", SynthesizeAsync).WithName("SynthesizeText");
        aiGroup.MapPost("/sentence", SentenceGenerateAsync).WithName("GenerateSentence");
        aiGroup.MapPost("/sentence/split", SplitSentenceGenerateAsync).WithName("GenerateSplitSentence");

        #endregion

        return app;
    }

    private static async Task<IResult> GetChatsAsync(
        [FromRoute] Guid userId,
        [FromServices] IAccessorClient accessorClient,
        [FromServices] ILogger<ChatPostEndpoint> log,
        CancellationToken ct)
    {
        using var scope = log.BeginScope("userId: {UserId}", userId);
        {
            // TODO: Change userId from token
            try
            {
                var chats = await accessorClient.GetChatsForUserAsync(userId, ct);
                if (chats is null || !chats.Any())
                {
                    log.LogInformation("chats not found");
                    return Results.NotFound(new { error = "Chats not found" });
                }

                log.LogInformation("Chats returned");
                return Results.Ok(new { chats = chats });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to retrieve chats");
                return Results.Problem("Get chats retrieval failed");
            }
        }
    }

    private static async Task<IResult> GetChatHistoryAsync(
    [FromRoute] Guid chatId,
    [FromRoute] Guid userId,
    [FromServices] IEngineClient engineClient,
    [FromServices] ILogger<ChatPostEndpoint> log,
    CancellationToken ct)
    {
        using var scope = log.BeginScope("ChatId: {ChatId}, userId: {UserId}", chatId, userId);
        {
            // TODO: Change userId from token
            try
            {
                var history = await engineClient.GetHistoryChatAsync(chatId, userId, ct);
                if (history is null)
                {
                    log.LogInformation("chat history not found");
                    return Results.NotFound(new { error = "Chat history not found" });
                }

                log.LogInformation("Chat histiry returned");
                return Results.Ok(history);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to retrieve chats");
                return Results.Problem("Get chats retrieval failed");
            }
        }
    }

    private static async Task<IResult> ChatAsync(
      [FromBody] ChatRequest request,
      [FromServices] IEngineClient engine,
      [FromServices] ILogger<ChatPostEndpoint> log,
      CancellationToken ct)
    {
        using var scope = log.BeginScope("ThreadId: {ThreadId}", request.ThreadId);
        if (string.IsNullOrWhiteSpace(request.UserMessage))
        {
            return Results.BadRequest(new { error = "userMessage is required" });
        }

        if (string.IsNullOrWhiteSpace(request.ThreadId))
        {
            return Results.BadRequest(new { error = "threadId is required" });
        }

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Results.BadRequest(new { error = "userId is required" });
        }

        var requestId = Guid.NewGuid().ToString("N");

        //todo: takeUserId from token.

        if (!TryResolveThreadId(request.ThreadId, out var threadId, out var errorThread))
        {
            return Results.BadRequest(new { error = "If threadId is present, it must be a GUID" });
        }

        if (!TryResolveThreadId(request.UserId, out var userId, out var errorUser))
        {
            return Results.BadRequest(new { error = "If userId is present, it must be a GUID" });
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
            var engineResponse = await engine.ChatAsync(engineRequest);

            if (engineResponse.success)
            {
                log.LogInformation("Request {RequestId} (thread {Thread}) accepted", requestId, threadId);
                return Results.Ok(new
                {
                    requestId
                });
            }
            else
            {
                log.LogError("Engine chat failed - service returned failure for request {RequestId}", requestId);
                return Results.Problem(
                    title: "Engine chat failed",
                    detail: "Upstream service returned an error.",
                    statusCode: StatusCodes.Status502BadGateway,
                    extensions: new Dictionary<string, object?>
                    {
                        ["upstreamStatus"] = 500,
                        ["requestId"] = engineRequest.RequestId
                    });
            }
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
        HttpRequest req,
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
            if (engineResult == null)
            {
                logger.LogError("Engine synthesis failed - service returned null");
                return Results.Problem("Speech synthesis failed.");
            }

            var wantsBinary =
                req.Headers.Accept.Any(h => h != null && h.Contains("application/octet-stream", StringComparison.OrdinalIgnoreCase)) ||
                req.Headers.Accept.Any(h => h != null && h.Contains("audio/", StringComparison.OrdinalIgnoreCase)) ||
                (req.Query.TryGetValue("format", out var fmt) && string.Equals(fmt, "binary", StringComparison.OrdinalIgnoreCase));

            if (wantsBinary && !string.IsNullOrWhiteSpace(engineResult.AudioData))
            {
                var audioBytes = Convert.FromBase64String(engineResult.AudioData);
                var contentType = !string.IsNullOrWhiteSpace(engineResult.Metadata?.ContentType)
                    ? engineResult.Metadata.ContentType
                    : "audio/mpeg";

                logger.LogInformation("Returning binary audio (length {Length}, type {Type})", audioBytes.Length, contentType);
                return Results.File(audioBytes, contentType: contentType, fileDownloadName: null, enableRangeProcessing: true);
            }

            var response = new SpeechResponse
            {
                AudioData = engineResult.AudioData,
                Visemes = engineResult.Visemes,
                Metadata = engineResult.Metadata,
            };

            logger.LogInformation("Speech synthesis completed successfully");
            return Results.Ok(response);
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
    private static async Task<IResult> SentenceGenerateAsync(
       [FromBody] SentenceRequestDto dto,
       [FromServices] IEngineClient engineClient,
       [FromServices] ILogger<SpeechEndpoints> logger,
       HttpContext httpContext,
       CancellationToken ct)
    {
        if (dto is null)
        {
            return Results.BadRequest(new { error = "Request is required" });
        }

        logger.LogInformation("Received sentence generation request");

        try
        {
            var userId = GetUserId(httpContext, logger);

            var request = new SentenceRequest
            {
                Difficulty = dto.Difficulty,
                Nikud = dto.Nikud,
                Count = dto.Count,
                UserId = userId
            };

            await engineClient.GenerateSentenceAsync(request);
            return Results.Ok();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Sentence generation operation was canceled by user");
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Sentence generation operation timed out");
            return Results.Problem("Sentence generation is taking too long.", statusCode: StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in Sentence generation manager");
            return Results.Problem("An error occurred during sentence generation.");
        }
    }
    private static async Task<IResult> SplitSentenceGenerateAsync(
       [FromBody] SentenceRequestDto dto,
       [FromServices] IEngineClient engineClient,
       [FromServices] ILogger<SpeechEndpoints> logger,
       HttpContext httpContext,
       CancellationToken ct)
    {
        if (dto is null)
        {
            return Results.BadRequest(new { error = "Request is required" });
        }

        logger.LogInformation("Received split sentence generation request");

        try
        {
            var userId = GetUserId(httpContext, logger);

            var request = new SentenceRequest
            {
                Difficulty = dto.Difficulty,
                Nikud = dto.Nikud,
                Count = dto.Count,
                UserId = userId
            };

            await engineClient.GenerateSplitSentenceAsync(request);
            return Results.Ok();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Split sentence generation operation was canceled by user");
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Split sentence generation operation timed out");
            return Results.Problem("Split sentence generation is taking too long.", statusCode: StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in split sentence generation manager");
            return Results.Problem("An error occurred during split sentence generation.");
        }
    }
    private static Guid GetUserId(HttpContext httpContext, ILogger logger)
    {
        var raw = httpContext?.User?.Identity?.Name;

        if (!Guid.TryParse(raw, out var userId))
        {
            logger.LogError("Missing or invalid UserId in HttpContext. Raw: {RawUserId}", raw);
            throw new InvalidOperationException("Authenticated user id is missing or not a valid GUID.");
        }

        return userId;
    }
}