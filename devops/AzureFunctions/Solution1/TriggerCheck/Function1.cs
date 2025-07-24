using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace TriggerCheck;

public class Function1
{
    private readonly ILogger<Function1> _logger;

    public Function1(ILogger<Function1> logger)
    {
        _logger = logger;
    }

    [Function(nameof(Function1))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", "get")] HttpRequestData req)
    {
        _logger.LogInformation("HTTP trigger function processed a request.");

        // Example: Read request body (for POST)
        string requestBody = await req.ReadAsStringAsync();
        _logger.LogInformation("Request Body: {body}", requestBody);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("HTTP trigger executed successfully.");
        return response;
    }
}