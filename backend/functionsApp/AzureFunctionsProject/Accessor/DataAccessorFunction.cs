using AzureFunctionsProject.Common;
using AzureFunctionsProject.Models;
using AzureFunctionsProject.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.Accessor
{
    public class DataAccessorFunction
    {
        private readonly IDataService _service;
        private readonly ILogger<DataAccessorFunction> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public DataAccessorFunction(IDataService service, ILogger<DataAccessorFunction> logger)
        {
            _service = service;
            _logger = logger;
        }

        [Function("AccessorGetAllData")]
        public async Task<HttpResponseData> GetAll(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.AccessorGetAll)]
            HttpRequestData req)
        {
            var resp = req.CreateResponse();
            try
            {
                var list = await _service.GetAllAsync();
                resp.StatusCode = HttpStatusCode.OK;
                await resp.WriteAsJsonAsync(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Accessor GetAllData error");
                resp.StatusCode = HttpStatusCode.InternalServerError;
                await resp.WriteStringAsync("Error fetching data");
            }
            return resp;
        }

        [Function("AccessorGetDataById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.AccessorGetById)]
            HttpRequestData req,
            string id)
        {
            var resp = req.CreateResponse();
            if (!Guid.TryParse(id, out var guid))
            {
                resp.StatusCode = HttpStatusCode.BadRequest;
                await resp.WriteStringAsync("Invalid GUID");
                return resp;
            }

            try
            {
                var dto = await _service.GetByIdAsync(guid);
                if (dto is null)
                {
                    resp.StatusCode = HttpStatusCode.NotFound;
                    return resp;
                }

                resp.StatusCode = HttpStatusCode.OK;
                resp.Headers.Add("ETag", $"\"{dto.Version}\"");
                await resp.WriteAsJsonAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Accessor GetDataById error for {Id}", id);
                resp.StatusCode = HttpStatusCode.InternalServerError;
                await resp.WriteStringAsync("Error fetching data");
            }
            return resp;
        }

        [Function("AccessorProcessQueue")]
        public async Task ProcessQueueAsync(
            [ServiceBusTrigger(Queues.Incoming, Connection = "ServiceBusConnection")]
            string messageBody)
        {
            var wrapper = JsonSerializer.Deserialize<QueueMessage>(messageBody, JsonOptions)
                          ?? throw new InvalidOperationException("Invalid queue message");

            try
            {
                switch (wrapper.Action)
                {
                    case "Create":
                        await _service.CreateAsync(wrapper.Entity!);
                        break;
                    case "Update":
                        await _service.UpdateAsync(wrapper.Entity!);
                        break;
                    case "Delete":
                        await _service.DeleteAsync(wrapper.Id!.Value);
                        break;
                    default:
                        _logger.LogWarning("Unknown action in queue: {Action}", wrapper.Action);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing queue message");
                throw;
            }
        }
    }
}
