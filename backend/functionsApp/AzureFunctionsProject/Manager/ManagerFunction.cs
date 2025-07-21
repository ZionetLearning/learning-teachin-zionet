using Azure.Messaging.ServiceBus;
using AzureFunctionsProject.Common;
using AzureFunctionsProject.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.Manager
{
    /// <summary>
    /// HTTP API front-end for enqueuing and retrieving generic Data entities via Service Bus and PostgreSQL.
    /// </summary>
    public sealed class DataAccessorFunction
    {
        private readonly ServiceBusSender _queueSender;
        private readonly IAccessorClient _accessor;
        private readonly ILogger<DataAccessorFunction> _logger;

        /// <summary>
        /// Constructor: injects ServiceBusClient, DB factory, and Logger.
        /// </summary>
        public DataAccessorFunction(
            ServiceBusClient sbClient,
            IAccessorClient accessor,
            ILogger<DataAccessorFunction> logger)
        {
            var queueName = Environment.GetEnvironmentVariable("IncomingQueueName")
                            ?? throw new InvalidOperationException("IncomingQueueName is not configured");
            _queueSender = sbClient.CreateSender(queueName);
            _accessor = accessor;
            _logger = logger;
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// 1. GET /api/data
        /// Retrieves all Data records via the accessor.
        /// </summary>
        [Function("GetAllData")]
        public async Task<HttpResponseData> GetAll(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.ManagerGetAll)]
            HttpRequestData req)
        {
            var respOut = req.CreateResponse();
            try
            {
                // call the accessor function
                var list = await _accessor.GetAllDataAsync();

                respOut.StatusCode = HttpStatusCode.OK;
                await respOut.WriteAsJsonAsync(list);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager GetAllData error");
                respOut.StatusCode = HttpStatusCode.InternalServerError;
                await respOut.WriteStringAsync("Error retrieving data");
            }

            return respOut;
        }


        /// <summary>
        /// 2. GET /api/data/{id}
        /// Retrieves a single Data record by ID via the accessor.
        /// </summary>
        [Function("GetDataById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.ManagerGetById)]
            HttpRequestData req,
            string id)
        {
            var respOut = req.CreateResponse();
            if (!Guid.TryParse(id, out var guid))
            {
                respOut.StatusCode = HttpStatusCode.BadRequest;
                await respOut.WriteStringAsync("Invalid GUID");
                return respOut;
            }

            try
            {
                var dto = await _accessor.GetDataByIdAsync(guid);
                if (dto is null)
                {
                    respOut.StatusCode = HttpStatusCode.NotFound;
                    return respOut;
                }

                respOut.StatusCode = HttpStatusCode.OK;
                respOut.Headers.Add("ETag", $"\"{dto.Version}\"");
                await respOut.WriteAsJsonAsync(dto);
            }
            catch (HttpRequestException hre) when (hre.StatusCode == HttpStatusCode.NotFound)
            {
                respOut.StatusCode = HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager GetById error for {Id}", id);
                respOut.StatusCode = HttpStatusCode.InternalServerError;
                await respOut.WriteStringAsync("Error retrieving data");
            }
            return respOut;
        }

        /// <summary>
        /// 3. POST /api/data
        /// Enqueues a CREATE action for a new Data record.
        /// </summary>
        [Function("CreateData")]
        public async Task<HttpResponseData> Create(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Routes.ManagerCreate)]
            HttpRequestData req)
        {
            DataDto dto;
            try
            {
                dto = await JsonSerializer.DeserializeAsync<DataDto>(req.Body,JsonOptions)
                    ?? throw new InvalidDataException();
            }
            catch
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid JSON body");
                return bad;
            }

            dto.Id = Guid.NewGuid();
            dto.Version = 0;

            var messageBody = JsonSerializer.Serialize(new
            {
                Action = "Create",
                Entity = dto
            });

            try
            {
                _logger.LogInformation("Enqueuing CREATE for {DataId}", dto.Id);
                var msg = new ServiceBusMessage(messageBody)
                {
                    MessageId = dto.Id.ToString(),
                    Subject = "CreateData"
                };
                await _queueSender.SendMessageAsync(msg);

                var resp = req.CreateResponse(HttpStatusCode.Accepted);
                resp.Headers.Add("Location", $"/api/data/{dto.Id}");
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue CREATE for {DataId}", dto.Id);
                var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await resp.WriteStringAsync("Could not enqueue create");
                return resp;
            }
        }

        /// <summary>
        /// 4. PUT /api/data/{id}
        /// Enqueues an UPDATE action, requiring If-Match header for concurrency.
        /// </summary>
        [Function("UpdateData")]
        public async Task<HttpResponseData> Update(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Routes.ManagerUpdate)]
            HttpRequestData req,
            string id)
        {
            if (!Guid.TryParse(id, out var guid))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            // Parse ETag for optimistic concurrency
            if (!req.Headers.TryGetValues("If-Match", out var etags) ||
                !uint.TryParse(etags.First().Trim('"'), out var incomingVersion))
            {
                var pre = req.CreateResponse(HttpStatusCode.PreconditionRequired);
                await pre.WriteStringAsync("Missing or invalid If-Match header");
                return pre;
            }

            DataDto dto;
            try
            {
                dto = await JsonSerializer.DeserializeAsync<DataDto>(req.Body,JsonOptions)
                    ?? throw new InvalidDataException();
            }
            catch
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid JSON body");
                return bad;
            }

            dto.Id = guid;
            dto.Version = incomingVersion;

            var messageBody = JsonSerializer.Serialize(new
            {
                Action = "Update",
                Entity = dto
            });

            try
            {
                _logger.LogInformation("Enqueuing UPDATE for {DataId} @v{Version}",
                                        dto.Id, dto.Version);
                var msg = new ServiceBusMessage(messageBody)
                {
                    MessageId = $"{dto.Id}:{dto.Version}",
                    Subject = "UpdateData"
                };
                await _queueSender.SendMessageAsync(msg);

                return req.CreateResponse(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue UPDATE for {DataId}", dto.Id);
                var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await resp.WriteStringAsync("Could not enqueue update");
                return resp;
            }
        }

        /// <summary>
        /// 5. DELETE /api/data/{id}
        /// Enqueues a DELETE action; optional If-Match for concurrency.
        /// </summary>
        [Function("DeleteData")]
        public async Task<HttpResponseData> Delete(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Routes.ManagerDelete)]
            HttpRequestData req,
            string id)
        {
            if (!Guid.TryParse(id, out var guid))
                return req.CreateResponse(HttpStatusCode.BadRequest);

            // Optional ETag support
            uint? version = null;
            if (req.Headers.TryGetValues("If-Match", out var etags) &&
                uint.TryParse(etags.First().Trim('"'), out var v))
                version = v;

            var messageBody = JsonSerializer.Serialize(new
            {
                Action = "Delete",
                Id = guid,
                Version = version
            });

            try
            {
                _logger.LogInformation("Enqueuing DELETE for {DataId} @v{Version}",
                                        guid, version);
                var msg = new ServiceBusMessage(messageBody)
                {
                    MessageId = version.HasValue
                        ? $"{guid}:{version}"
                        : guid.ToString(),
                    Subject = "DeleteData"
                };
                await _queueSender.SendMessageAsync(msg);

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue DELETE for {DataId}", guid);
                var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await resp.WriteStringAsync("Could not enqueue delete");
                return resp;
            }
        }
    }

}
