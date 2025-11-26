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

    public async Task<EmailDraftResponse> GenerateDraftAsync(string emailPromptContent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(emailPromptContent))
        {
            _log.LogError("Prompt is empty for email draft generation");
            throw new ArgumentException("Prompt cannot be empty", nameof(emailPromptContent));
        }

        _log.LogInformation("Generating email draft with direct prompt invocation");

        var exec = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.7,
            ResponseFormat = typeof(EmailDraftResponse)
        };

        var args = new KernelArguments(exec);

        var result = await _kernel.InvokePromptAsync(emailPromptContent, args, null, null, null, ct);

        var json = result.GetValue<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            _log.LogError("Empty response for email draft generation");
            throw new RetryableException("Empty response from model");
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<EmailDraftResponse>(json);

            if (parsed == null || string.IsNullOrWhiteSpace(parsed.Subject) || string.IsNullOrWhiteSpace(parsed.Body))
            {
                _log.LogError("Invalid or incomplete email draft response: {Json}", json);
                throw new RetryableException("Incomplete model response");
            }

            return parsed;
        }
        catch (JsonException ex)
        {
            _log.LogError(ex, "Failed to parse model output JSON: {Json}", json);
            throw new RetryableException("Malformed JSON output from model", ex);
        }
    }
}

