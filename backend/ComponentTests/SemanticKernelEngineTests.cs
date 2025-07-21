using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.ComponentTests
{
    public class SemanticKernelEngineTests
    {
        private readonly IChatCompletionService _chatService;

        public SemanticKernelEngineTests()
        {
            string? apiKeyNullable = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKeyNullable))
            {
                var config = new ConfigurationBuilder()
                    .AddUserSecrets<SemanticKernelEngineTests>(optional: true)
                    .Build();

                apiKeyNullable = config["OpenAI:ApiKey"];
            }

            if (string.IsNullOrWhiteSpace(apiKeyNullable))
            {
                throw new InvalidOperationException(
                    "Не найден OPENAI_API_KEY: задайте переменную окружения или user‑secret");
            }

            string apiKey = apiKeyNullable!;

            var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_ID")
                          ?? "gpt-4o-mini";

            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                modelId: modelId,
                apiKey: apiKey,
                serviceId: "openai");

            var kernel = builder.Build();
            _chatService = kernel.Services.GetRequiredService<IChatCompletionService>();
        }

        [Fact(DisplayName = "ChatCompletion returns a response with '4' or 'four'")]
        public async Task ChatCompletion_ReturnsExpectedAnswer()
        {
            // arrange
            var history = new ChatHistory();
            history.AddUserMessage("What is 2 plus 2?");

            // act
            var results = await _chatService.GetChatMessageContentsAsync(history);

            // assert
            Assert.NotEmpty(results);
            var response = results[0].Content;
            Assert.False(string.IsNullOrWhiteSpace(response));

            bool hasDigit = response.Contains("4");
            bool hasWord = response.IndexOf("four", StringComparison.OrdinalIgnoreCase) >= 0;

            Assert.True(
                hasDigit || hasWord,
                $"Expected '4' or 'four' in response, but got: {response}");
        }
    }
}