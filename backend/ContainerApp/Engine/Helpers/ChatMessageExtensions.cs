using System.Text.Json;
using Engine.Models.Chat;
using Microsoft.Extensions.AI;

namespace Engine.Helpers;

public static class ChatMessageExtensions
{
    public static OpenAiMessageDto ToOpenAiDto(this ChatMessage msg)
    {
        var role = msg.Role.Value.ToLowerInvariant();

        var msgId = Guid.NewGuid().ToString();
        if (msg.AdditionalProperties is not null &&
            msg.AdditionalProperties.TryGetValue("Id", out var existingIdObj))
        {
            msgId = existingIdObj?.ToString() ?? msgId;
        }

        var textContent = string.Concat(msg.Contents.OfType<TextContent>().Select(t => t.Text));

        List<OpenAiToolCallDto>? toolCalls = null;
        var funcCalls = msg.Contents.OfType<FunctionCallContent>().ToList();

        if (funcCalls.Any())
        {
            toolCalls = funcCalls.Select(fc => new OpenAiToolCallDto
            {
                Id = fc.CallId,
                Function = new OpenAiFunctionDto
                {
                    Name = fc.Name,
                    Arguments = JsonSerializer.Serialize(fc.Arguments)
                }
            }).ToList();
        }

        var toolResult = msg.Contents.OfType<FunctionResultContent>().FirstOrDefault();
        var toolCallId = toolResult?.CallId;
        var toolResText = toolResult?.Result?.ToString();

        if (role == "tool" && string.IsNullOrEmpty(textContent))
        {
            textContent = toolResText ?? "";
        }

        return new OpenAiMessageDto
        {
            Id = msgId,
            Role = role,
            Content = textContent,
            ToolCalls = toolCalls,
            ToolCallId = toolCallId
        };
    }
}