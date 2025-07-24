using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace Manager;

public class ConfirmSaveFunction
{
    private readonly ILogger<ConfirmSaveFunction> _logger;

    public ConfirmSaveFunction(ILogger<ConfirmSaveFunction> logger)
    {
        _logger = logger;
    }

    [Function("ConfirmSave")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "confirm")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                _logger.LogWarning("[Manager] Empty confirmation body received.");
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Request body is empty.");
                return badRequest;
            }

            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);

            if (data == null || !data.ContainsKey("status"))
            {
                _logger.LogWarning("[Manager] Invalid confirmation payload.");
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid or missing confirmation data.");
                return badRequest;
            }

            _logger.LogInformation("[Manager] Confirmation received from Accessor:");
            foreach (var kvp in data)
            {
                _logger.LogInformation($"{kvp.Key}: {kvp.Value}");
            }

            var res = req.CreateResponse(HttpStatusCode.OK);
            await res.WriteStringAsync("Confirmation received by Manager.");
            return res;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"[Manager] JSON parse error: {ex.Message}");
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("Malformed JSON.");
            return badRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError($"[Manager] Unexpected error: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Internal server error.");
            return errorResponse;
        }
    }
}