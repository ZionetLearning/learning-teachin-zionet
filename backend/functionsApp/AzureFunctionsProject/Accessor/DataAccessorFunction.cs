using Azure.Messaging.ServiceBus;
using AzureFunctionsProject.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Net;
using System.Text.Json;

namespace AzureFunctionsProject.Accessor
{
    public class DataAccessorFunction
    {
        private readonly Func<NpgsqlConnection> _dbFactory;
        private readonly ILogger<DataAccessorFunction> _logger;
        private readonly string _devTag;
        public DataAccessorFunction(
            Func<NpgsqlConnection> dbFactory,
            ILogger<DataAccessorFunction> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
            _devTag = Environment.MachineName;
        }

        [Function("AccessorGetAllData")]
        public async Task<HttpResponseData> GetAll(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.AccessorGetAll)]
            HttpRequestData req)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);

        }

        [Function("AccessorGetDataById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = Routes.AccessorGetById)]
            HttpRequestData req,
            string id)
        {
                return req.CreateResponse(HttpStatusCode.BadRequest);

        }
        [Function("AccessorProcessQueue")]
        public async Task ProcessQueueAsync(
        [ServiceBusTrigger(Queues.Incoming, Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions actions,
        FunctionContext context)
        {
            string body = message.Body.ToString();
            string msgDevTag = message.ApplicationProperties.TryGetValue("DevTag", out var tag)
                ? tag?.ToString() ?? "unknown"
                : "unknown";

            if (msgDevTag == _devTag)
            {
                _logger.LogInformation("Processed: {Body} (Dev={Dev})", body, msgDevTag);
                await actions.CompleteMessageAsync(message);
            }
            else
            {
                _logger.LogInformation("Skipped: {Body} (Dev={Dev})", body, msgDevTag);
                await actions.AbandonMessageAsync(message);
            }
        }
    }
}
