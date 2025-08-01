﻿using Microsoft.SemanticKernel;
using Engine.Models;

namespace Engine.Services;

public sealed class ChatAiService : IChatAiService
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatAiService> _log;

    public ChatAiService(Kernel kernel, ILogger<ChatAiService> log)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _log = log ?? throw new ArgumentNullException(nameof(log));
    }

    public async Task<AiResponseModel> ProcessAsync(AiRequestModel request, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now > request.SentAt + request.TtlSeconds)
        {
            _log.LogWarning("Request {Id} is expired. Skipping.", request.Id);
            return new AiResponseModel
            {
                Id = request.Id,
                Status = "expired",
                Error = "TTL expired"
            };
        }

        _log.LogInformation("AI processing request {Id}", request.Id);

        try
        {
            var func = _kernel.CreateFunctionFromPrompt(
                "Answer the question:\n{{$input}}");

            var args = new KernelArguments
            {
                ["input"] = request.Question
            };

            var answer = await _kernel.InvokeAsync<string>(func, args, ct) ?? string.Empty;

            return new AiResponseModel
            {
                Id = request.Id,
                Answer = answer
            };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error while processing AI request {Id}", request.Id);
            return new AiResponseModel
            {
                Id = request.Id,
                Status = "error",
                Error = ex.Message
            };
        }
    }
}