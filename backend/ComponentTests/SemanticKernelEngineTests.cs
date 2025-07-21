using System.IO;
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
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public SemanticKernelEngineTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            var openAiSection = config.GetSection("OpenAI");
            //var endpoint = openAiSection["Endpoint"];
            var apiKey = openAiSection["ApiKey"];
            var modelId = openAiSection["ModelId"] ?? "gpt-4o-mini";

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidDataException(
                    "There is no ApiKey in the OpenAI section of appsettings.json"
                );

            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(modelId, apiKey);

            _kernel = builder.Build();
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        [Fact(DisplayName = "ChatCompletion returns a response with the number 4")]
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
            bool hasWord = response
                .IndexOf("four", StringComparison.OrdinalIgnoreCase) >= 0;

            Assert.True(
                hasDigit || hasWord,
                $"The response was expected to contain '4' or 'four', but received:{response}"
            );
        }
    }
}