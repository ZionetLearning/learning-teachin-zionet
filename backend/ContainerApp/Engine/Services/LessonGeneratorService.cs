using System.Text.Json;
using Engine.Models.Lessons;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace Engine.Services;

public class LessonGeneratorService : ILessonGeneratorService
{
    private readonly Kernel _kernel;
    private readonly ILogger<LessonGeneratorService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LessonGeneratorService(
        [FromKeyedServices("lessons")] Kernel kernel,
        ILogger<LessonGeneratorService> logger)
    {
        _kernel = kernel;
        _logger = logger;
    }

    public async Task<EngineLessonResponse> GenerateLessonAsync(EngineLessonRequest request, CancellationToken ct = default)
    {
        _logger.LogInformation("Generating lesson for topic: {Topic}", request.Topic);

        var func = _kernel.Plugins["Lessons"]["Generate"];

        var execSettings = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.7,
            ResponseFormat = typeof(EngineLessonResponse)
        };

        var args = new KernelArguments(execSettings)
        {
            ["topic"] = request.Topic
        };

        var result = await _kernel.InvokeAsync(func, args, ct);
        var json = result.GetValue<string>();

        if (string.IsNullOrWhiteSpace(json))
        {
            _logger.LogError("AI returned empty response for topic: {Topic}", request.Topic);
            throw new InvalidOperationException("AI returned an empty response");
        }

        var lesson = JsonSerializer.Deserialize<EngineLessonResponse>(json, JsonOptions);

        if (lesson is null)
        {
            _logger.LogError("Failed to parse AI response for topic: {Topic}. Response: {Response}", request.Topic, json);
            throw new InvalidOperationException("Failed to parse AI response");
        }

        if (lesson.ContentSections is null || lesson.ContentSections.Count == 0)
        {
            _logger.LogError("AI returned lesson with no content sections for topic: {Topic}", request.Topic);
            throw new InvalidOperationException("AI returned a lesson with no content sections");
        }

        _logger.LogInformation(
            "Successfully generated lesson '{Title}' with {SectionCount} sections for topic: {Topic}",
            lesson.Title,
            lesson.ContentSections.Count,
            request.Topic);

        return lesson;
    }
}

