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
        private readonly AzureFunctionsProject.Manager.IAccessorClient _accessor;
        private readonly ILogger<EngineFunction> _logger;

        public EngineFunction(
            AzureFunctionsProject.Manager.IAccessorClient accessor,
            ILogger<EngineFunction> logger)
        {
            _accessor = accessor;
            _logger = logger;
        }

        [Function("ProcessDataEngine")]
        public async Task<HttpResponseData> Process(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.EngineProcess)]
            HttpRequestData req)
        {
            var resp = req.CreateResponse();
            try
            {
                // 1. fetch everything via the accessor
                var all = await _accessor.GetAllDataAsync();

                // 2. do your operation—here we just count rows
                var result = new { TotalCount = all.Count };

                resp.StatusCode = HttpStatusCode.OK;
                await resp.WriteAsJsonAsync(result);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: GET data failed");
                throw new AccessorClientException(
                    $"Failed to retrieve Data from the Accessor service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Engine processing failed");
                resp.StatusCode = HttpStatusCode.InternalServerError;
                await resp.WriteStringAsync("Error in engine");
            }
            return resp;
        }
    }
}
