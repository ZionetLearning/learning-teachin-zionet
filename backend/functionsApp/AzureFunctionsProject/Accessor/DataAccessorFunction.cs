using AzureFunctionsProject.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Net;

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
    }
}
