using System.Text.Json;
using DotQueue;
using Engine.Models.Emails;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Engine.Services;

public class EmailService : IEmailService
{
    private readonly Kernel _kernel;
    private readonly ILogger<EmailService> _log;

    public EmailService([FromKeyedServices("gen")] Kernel kernel, ILogger<EmailService> log)
    {
        _kernel = kernel;
        _log = log;
    }

    public async Task<EmailDraftResponse> GenerateDraftAsync(EmailDraftRequest request, CancellationToken ct = default)
    {
        _log.LogInformation("Inside email draft generation service, Purpose={Purpose}", request.Purpose);

        var func = _kernel.Plugins["Email"]["GenerateDraft"];

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.7,
            ResponseFormat = typeof(EmailDraftResponse)
        };

        var args = new KernelArguments(exec)
        {
            ["purpose"] = request.Purpose,
            ["notes"] = request.Notes
        };

        var result = await _kernel.InvokeAsync(func, args, ct);
        var json = result.GetValue<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            _log.LogError("Empty response for email draft generation");
            throw new RetryableException("Empty response for email draft generation");
        }

        var parsed = JsonSerializer.Deserialize<EmailDraftResponse>(json);
        if (parsed is null)
        {
            _log.LogError("Invalid JSON for email draft: {Json}", json);
            throw new RetryableException("Invalid JSON format from model");
        }

        return parsed;
    }
}

