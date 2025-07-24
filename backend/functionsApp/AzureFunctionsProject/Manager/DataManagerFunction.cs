using Azure.Messaging.ServiceBus;
using AzureFunctionsProject.Common;
using AzureFunctionsProject.Exceptions;
using AzureFunctionsProject.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Threading;

namespace AzureFunctionsProject.Manager
{
    /// <summary>
    /// HTTP API front-end for enqueuing and retrieving generic Data entities via Service Bus and PostgreSQL.
    /// </summary>
    public sealed class DataManagerFunction
    {
        private readonly ServiceBusSender _queueSender;
        private readonly IAccessorClient _accessor;
        private readonly IEngineClient _engine;
        private readonly ILogger<DataManagerFunction> _logger;

        /// <summary>
        /// Constructor: injects ServiceBusClient, DB factory, and Logger.
        /// </summary>
        public DataManagerFunction(
            ServiceBusClient sbClient,
            IAccessorClient accessor,
            IEngineClient engine,
            ILogger<DataManagerFunction> logger)
        {
            _queueSender = sbClient.CreateSender(Queues.Incoming);
            _accessor = accessor;
            _engine = engine;
            _logger = logger;
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        ///  GET /api/data
        /// Retrieves all Data records via the accessor.
        /// </summary>
        [Function("GetAllData")]
        public async Task<HttpResponseData> GetAllAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.ManagerGetAll)]
            HttpRequestData req, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GET /api/data");
            var respOut = req.CreateResponse();
            try
            {
                var list = await _accessor.GetAllDataAsync(cancellationToken);
                respOut.StatusCode = HttpStatusCode.OK;
                await respOut.WriteAsJsonAsync(list, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: GET data failed");
                throw new AccessorClientException(
                    $"Failed to retrieve Data from the Accessor service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager GetAllData error");
                respOut.StatusCode = HttpStatusCode.InternalServerError;
                await respOut.WriteStringAsync("Error retrieving data", cancellationToken);
            }

            return respOut;
        }


        /// <summary>
        ///  GET /api/data/{id}
        /// Retrieves a single Data record by ID via the accessor.
        /// </summary>
        [Function("GetDataById")]
        public async Task<HttpResponseData> GetByIdAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.ManagerGetById)]
            HttpRequestData req,
            string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GET /api/data/{Id}", id);
            var respOut = req.CreateResponse();
            if (!Guid.TryParse(id, out var guid))
            {
                respOut.StatusCode = HttpStatusCode.BadRequest;
                await respOut.WriteStringAsync("Invalid GUID", cancellationToken);
                return respOut;
            }

            try
            {
                var dto = await _accessor.GetDataByIdAsync(guid, cancellationToken);
                if (dto is null)
                {
                    respOut.StatusCode = HttpStatusCode.NotFound;
                    return respOut;
                }

                respOut.StatusCode = HttpStatusCode.OK;
                respOut.Headers.Add("ETag", $"\"{dto.Version}\"");
                await respOut.WriteAsJsonAsync(dto, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Accessor: GET data failed");
                throw new AccessorClientException(
                    $"Failed to retrieve Data from the Accessor service", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager GetById error for {Id}", id);
                respOut.StatusCode = HttpStatusCode.InternalServerError;
                await respOut.WriteStringAsync("Error retrieving data", cancellationToken);
            }
            return respOut;
        }

         /// <summary>
        /// GET /api/data/process
        ///  calls EngineFunction, returns processed result.
        /// </summary>
        [Function("ProcessData")]
        public async Task<HttpResponseData> ProcessDataAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.ManagerProcessData)]
            HttpRequestData req, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling GET /api/data/process");
            var outResp = req.CreateResponse();
            try
            {
                var result = await _engine.ProcessDataAsync(cancellationToken);
                outResp.StatusCode = HttpStatusCode.OK;
                await outResp.WriteAsJsonAsync(result, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manager to Engine process failed");
                outResp.StatusCode = HttpStatusCode.InternalServerError;
                await outResp.WriteStringAsync("Error processing data", cancellationToken);
            }
            return outResp;
        }
        /// <summary>
        /// POST /api/data
        /// Enqueues a CREATE action for a new Data record.
        /// </summary>
        [Function("CreateData")]
        public async Task<HttpResponseData> CreateDataAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = Routes.ManagerCreate)]
    HttpRequestData req,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received CreateData request POST /api/data");

            DataDto dto;
            try
            {
                dto = await JsonSerializer.DeserializeAsync<DataDto>(
                          req.Body, _jsonOptions, cancellationToken)
                      ?? throw new InvalidDataException();
            }
            catch (JsonException jex)
            {
                _logger.LogWarning(jex, "Invalid JSON body in CreateData");
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid JSON body", cancellationToken);
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
                    Subject = "CreateData",
                    TimeToLive = TimeSpan.FromMinutes(5)
                };
                // Use injected sender
                await _queueSender.SendMessageAsync(msg, cancellationToken);

                var resp = req.CreateResponse(HttpStatusCode.Accepted);
                resp.Headers.Add("Location", $"/api/data/{dto.Id}");
                return resp;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue CREATE for {DataId}", dto.Id);
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Could not enqueue create", cancellationToken);
                return error;
            }
        }

        /// <summary>
        ///  PUT /api/data/{id}
        /// Enqueues an UPDATE action, requiring If-Match header for concurrency.
        /// </summary>
        [Function("UpdateData")]
        public async Task<HttpResponseData> UpdateDataAsync(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = Routes.ManagerUpdate)]
            HttpRequestData req,
            string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling PUT /api/data/{Id}", id);
            if (!Guid.TryParse(id, out var guid))
            {
                var badId = req.CreateResponse(HttpStatusCode.BadRequest);
                await badId.WriteStringAsync("Invalid GUID", cancellationToken);
                return badId;
            }

            // Parse ETag for optimistic concurrency
            if (!req.Headers.TryGetValues("If-Match", out var etags) ||
                !uint.TryParse(etags.First().Trim('"'), out var incomingVersion))
            {
                var pre = req.CreateResponse(HttpStatusCode.PreconditionRequired);
                await pre.WriteStringAsync("Missing or invalid If-Match header", cancellationToken);
                return pre;
            }

            DataDto dto;
            try
            {
                dto = await JsonSerializer.DeserializeAsync<DataDto>(req.Body, _jsonOptions, cancellationToken)
                    ?? throw new InvalidDataException();
            }
            catch (JsonException jex)
            {
                _logger.LogWarning(jex, "Invalid JSON body in UpdateData");
                var badJson = req.CreateResponse(HttpStatusCode.BadRequest);
                await badJson.WriteStringAsync("Invalid JSON body", cancellationToken);
                return badJson;
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
                _logger.LogInformation("Enqueuing UPDATE for {DataId} @v{Version}",dto.Id, dto.Version);
                var msg = new ServiceBusMessage(messageBody)
                {
                    MessageId = $"{dto.Id}:{dto.Version}",
                    Subject = "UpdateData",
                    TimeToLive = TimeSpan.FromMinutes(5)
                };
                await _queueSender.SendMessageAsync(msg, cancellationToken);

                return req.CreateResponse(HttpStatusCode.Accepted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue UPDATE for {DataId}", dto.Id);
                var error = req.CreateResponse(HttpStatusCode.InternalServerError);
                await error.WriteStringAsync("Could not enqueue update", cancellationToken);
                return error;
            }
        }

        /// <summary>
        ///  DELETE /api/data/{id}
        /// Enqueues a DELETE action; optional If-Match for concurrency.
        /// </summary>
        [Function("DeleteData")]
        public async Task<HttpResponseData> DeleteDataAsync(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = Routes.ManagerDelete)]
            HttpRequestData req,
            string id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling DELETE /api/data/{Id}", id);

            if (!Guid.TryParse(id, out var guid))
            {
                var badId = req.CreateResponse(HttpStatusCode.BadRequest);
                await badId.WriteStringAsync("Invalid GUID", cancellationToken);
                return badId;
            }

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
                _logger.LogInformation("Enqueuing DELETE for {DataId} @v{Version}", guid, version);
                var msg = new ServiceBusMessage(messageBody)
                {
                    MessageId = version.HasValue
                        ? $"{guid}:{version}"
                        : guid.ToString(),
                    Subject = "DeleteData",
                    TimeToLive = TimeSpan.FromMinutes(5)
                };
                await _queueSender.SendMessageAsync(msg, cancellationToken);

                return req.CreateResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue DELETE for {DataId}", guid);
                var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
                await resp.WriteStringAsync("Could not enqueue delete", cancellationToken);
                return resp;
            }
        }

        [Function("Negotiate")]
        public static async Task<HttpResponseData> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = Routes.ManagerSignalRNegotiate)] HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = "serverless", ConnectionStringSetting = "AzureSignalRConnectionString")]
            SignalRConnectionInfo connectionInfo)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            var json = JsonSerializer.Serialize(connectionInfo, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await response.WriteStringAsync(json);
            return response;
        }

        [Function("GetDataBySignalR")]
        [SignalROutput(HubName = "serverless", ConnectionStringSetting = "AzureSignalRConnectionString")]
        public SignalRMessageAction GetDataBySignalR(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.ManagerSignalRSendData)] HttpRequestData _)
        {
            var dummy = new { time = DateTime.UtcNow, value = new Random().Next(1000) };
            var messageJson = JsonSerializer.Serialize(dummy);

            _logger.LogInformation("Sending dummy message: {0}", messageJson);
            // this will broadcast to all connected clients
            return new SignalRMessageAction("newMessage")
            {
                Arguments = new[] { dummy },
            };
        }
    }
}
