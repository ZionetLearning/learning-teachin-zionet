using System.Text.Json;
using Manager.Constants;
using Manager.Models.Chat;
using Manager.Models.ModelValidation;
using Manager.Models.Sentences;
using Manager.Models.Words;
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
    private sealed class GlobalChatEndpoint { }
    private sealed class SpeechEndpoints { }
    private sealed class ExplainMistakeEndpoint { }
    private sealed class WordsEndpoint { }

    public static WebApplication MapAiEndpoints(this WebApplication app)
    {
        var aiGroup = app.MapGroup("/ai-manager").WithTags("AI").RequireAuthorization(PolicyNames.AdminOrTeacherOrStudent);

        #region HTTP GET

        aiGroup.MapGet("/chats/{userId:guid}", GetChatsAsync).WithName("GetChats");
        aiGroup.MapGet("/chat/{chatId:guid}/{userId:guid}", GetChatHistoryAsync).WithName("GetChatHistory");

        #endregion

        #region HTTP POST

        aiGroup.MapPost("/chat", ChatAsync).WithName("Chat");
        aiGroup.MapPost("/global-chat", GlobalChatAsync).WithName("Globalchat");
        aiGroup.MapPost("/chat/mistake-explanation", ExplainMistakeAsync).WithName("ExplainMistake");
        aiGroup.MapPost("/sentence", SentenceGenerateAsync).WithName("GenerateSentence");
        aiGroup.MapPost("/sentence/split", SplitSentenceGenerateAsync).WithName("GenerateSplitSentence");
        aiGroup.MapPost("/word-explain", WordExplainAsync).WithName("GenerateWordExplain");

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
      HttpContext httpContext,
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

        var requestId = Guid.NewGuid().ToString("N");

        if (!TryResolveThreadId(request.ThreadId, out var threadId, out var errorThread))
        {
            return Results.BadRequest(new { error = "If threadId is present, it must be a GUID" });
        }

        var userId = GetUserId(httpContext, log);

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

    private static async Task<IResult> GlobalChatAsync(
      [FromBody] ChatRequest request,
      [FromServices] IEngineClient engine,
      [FromServices] IAccessorClient accessorClient,
      [FromServices] ILogger<GlobalChatEndpoint> log,
      HttpContext httpContext,
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

        var requestId = Guid.NewGuid().ToString("N");

        var userId = GetUserId(httpContext, log);

        if (!TryResolveThreadId(request.ThreadId, out var threadId, out var errorThread))
        {
            return Results.BadRequest(new { error = "If threadId is present, it must be a GUID" });
        }

        JsonElement? parsedPageContext = null;
        if (request.PageContext is not null)
        {
            var raw = request.PageContext.JsonContext;
            parsedPageContext = ToJsonElementOrNull(raw, out var ctxErr);
            if (ctxErr is not null)
            {
                return Results.BadRequest(new { error = ctxErr });
            }
        }

        EngineChatRequest? engineRequest = null;

        try
        {
            var user = await accessorClient.GetUserAsync(userId);

            if (user == null)
            {
                log.LogWarning("User {UserId} not found in accessor", userId);
                return Results.BadRequest(new { error = "User not found" });

            }

            var userDetail = new UserDetailForChat
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                HebrewLevelValue = user.HebrewLevelValue.ToString(),
                PreferredLanguageCode = user.PreferredLanguageCode.ToString(),
                Interests = user.Interests,
                Role = user.Role.ToString()

            };

            engineRequest = new EngineChatRequest
            {
                RequestId = requestId,
                ThreadId = threadId,
                UserId = userId,
                UserMessage = request.UserMessage.Trim(),
                ChatType = request.ChatType,
                UserDetail = userDetail,
                PageContext = parsedPageContext

            };

            var engineResponse = await engine.GlobalChatAsync(engineRequest);

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
                    ["requestId"] = engineRequest?.RequestId ?? requestId
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
        catch (InvalidOperationException ex) when (ex.Data.Contains("Tag") &&
                                          Equals(ex.Data["Tag"], "MissingOrInvalidUserId"))
        {
            logger.LogWarning(ex, "Invalid or missing UserId");
            return Results.Problem("Invalid or missing UserId", statusCode: StatusCodes.Status403Forbidden);
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
    private static async Task<IResult> ExplainMistakeAsync(
        [FromBody] ExplainMistakeRequest request,
        [FromServices] IEngineClient engine,
        [FromServices] ILogger<ExplainMistakeEndpoint> log,
        HttpContext httpContext,
        CancellationToken ct)
    {
        using var scope = log.BeginScope("AttemptId: {AttemptId}, ThreadId: {ThreadId}", request.AttemptId, request.ThreadId);

        if (!ValidationExtensions.TryValidate(request, out var validationErrors))
        {
            log.LogWarning("Validation failed for {Model}: {Errors}", nameof(ExplainMistakeRequest), validationErrors);
            return Results.BadRequest(new { errors = validationErrors });
        }

        if (request.AttemptId == Guid.Empty)
        {
            return Results.BadRequest(new { error = "attemptId cannot be empty" });
        }

        var requestId = Guid.NewGuid().ToString("N");

        try
        {
            var userId = GetUserId(httpContext, log);

            if (!TryResolveThreadId(request.ThreadId, out var threadId, out var errorThread))
            {
                return Results.BadRequest(new { error = "ThreadId must be a valid GUID" });
            }

            var engineRequest = new EngineExplainMistakeRequest
            {
                RequestId = requestId,
                ThreadId = threadId,
                UserId = userId,
                AttemptId = request.AttemptId,
                GameType = request.GameType,
                ChatType = request.ChatType
            };

            var engineResponse = await engine.ExplainMistakeAsync(engineRequest);

            if (engineResponse.success)
            {
                log.LogInformation("Explain mistake request {RequestId} (thread {Thread}) accepted", requestId, threadId);
                return Results.Ok(new
                {
                    requestId
                });
            }
            else
            {
                log.LogError("Engine explain mistake failed - service returned failure for request {RequestId}", requestId);
                return Results.Problem(
                    title: "Engine explain mistake failed",
                    detail: "Upstream service returned an error.",
                    statusCode: StatusCodes.Status502BadGateway,
                    extensions: new Dictionary<string, object?>
                    {
                        ["upstreamStatus"] = 500,
                        ["requestId"] = requestId
                    });
            }
        }
        catch (InvalidOperationException ex) when (ex.Data.Contains("Tag") &&
                                          Equals(ex.Data["Tag"], "MissingOrInvalidUserId"))
        {
            log.LogWarning(ex, "Invalid or missing UserId");
            return Results.Problem("Invalid or missing UserId", statusCode: StatusCodes.Status403Forbidden);
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
                    ["requestId"] = requestId
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

    private static async Task<IResult> WordExplainAsync(
    [FromBody] WordExplainRequestDto dto,
    [FromServices] IEngineClient engineClient,
    [FromServices] ILogger<WordsEndpoint> logger,
    HttpContext httpContext,
    CancellationToken ct)
    {
        if (dto is null)
        {
            return Results.BadRequest(new { error = "Request is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Word))
        {
            return Results.BadRequest(new { error = "Word is required" });
        }

        if (string.IsNullOrWhiteSpace(dto.Context))
        {
            return Results.BadRequest(new { error = "Context is required" });
        }

        logger.LogInformation("Received word explanation request for {Word}", dto.Word);

        try
        {
            var userId = GetUserId(httpContext, logger);

            var request = new WordExplainRequest
            {
                Word = dto.Word,
                Context = dto.Context,
                UserId = userId
            };

            await engineClient.GenerateWordExplainAsync(request, ct);
            logger.LogInformation("Word explanation request for {Word} sent to engine", dto.Word);

            return Results.Ok();
        }
        catch (InvalidOperationException ex) when (ex.Data.Contains("Tag") &&
                                           Equals(ex.Data["Tag"], "MissingOrInvalidUserId"))
        {
            logger.LogWarning(ex, "Invalid or missing UserId");
            return Results.Problem("Invalid or missing UserId", statusCode: StatusCodes.Status403Forbidden);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Word explanation operation was canceled by user");
            return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (TimeoutException)
        {
            logger.LogWarning("Word explanation operation timed out");
            return Results.Problem("Word explanation is taking too long.", statusCode: StatusCodes.Status408RequestTimeout);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in word explanation manager");
            return Results.Problem("An error occurred during word explanation.");
        }
    }

    private static Guid GetUserId(HttpContext httpContext, ILogger logger)
    {
        var raw = httpContext?.User?.Identity?.Name;

        if (!Guid.TryParse(raw, out var userId))
        {
            logger.LogError("Missing or invalid UserId in HttpContext. Raw: {RawUserId}", raw);
            var ex = new InvalidOperationException("Authenticated user id is missing or not a valid GUID.");
            ex.Data["Tag"] = "MissingOrInvalidUserId";
            throw ex;
        }

        return userId;
    }

    private static JsonElement? ToJsonElementOrNull(string? rawJson, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            return doc.RootElement.Clone();
        }
        catch (Exception ex)
        {
            error = $"Invalid pageContext.json: {ex.Message}";
            return null;
        }
    }
}