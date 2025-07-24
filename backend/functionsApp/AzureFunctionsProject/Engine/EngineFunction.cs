using AzureFunctionsProject.Common;
using AzureFunctionsProject.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AzureFunctionsProject.Engine
{
    public sealed class EngineFunction
    {
        private readonly Manager.IAccessorClient _accessor;
        private readonly ILogger<EngineFunction> _logger;

        public EngineFunction(
            Manager.IAccessorClient accessor,
            ILogger<EngineFunction> logger)
        {
            _accessor = accessor;
            _logger = logger;
        }

        [Function("ProcessDataEngine")]
        public async Task<HttpResponseData> ProcessDataAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.EngineProcess)]
            HttpRequestData req,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("EngineFunction: starting data processing");
            var response = req.CreateResponse();

            try
            {
                var all = await _accessor.GetAllDataAsync(cancellationToken);
                var result = new { TotalCount = all.Count };

                response.StatusCode = HttpStatusCode.OK;
                await response.WriteAsJsonAsync(result, cancellationToken);
            }
            catch (AccessorClientException ex)
            {
                _logger.LogError(ex, "EngineFunction: failed to retrieve data from Accessor");
                response.StatusCode = HttpStatusCode.BadGateway;
                await response.WriteStringAsync(
                    "Failed to retrieve data from accessor service",
                    cancellationToken
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EngineFunction: unexpected error");
                response.StatusCode = HttpStatusCode.InternalServerError;
                await response.WriteStringAsync("Error in engine processing", cancellationToken);
            }

            return response;
        }
    }
}
