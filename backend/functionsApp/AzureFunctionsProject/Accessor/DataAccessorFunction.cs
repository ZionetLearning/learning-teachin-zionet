using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AzureFunctionsProject.Accessor
{
    public class DataAccessorFunction
    {
        private readonly Func<NpgsqlConnection> _dbFactory;
        private readonly ILogger<DataAccessorFunction> _logger;

        public DataAccessorFunction(
            Func<NpgsqlConnection> dbFactory,
            ILogger<DataAccessorFunction> logger)
        {
            _dbFactory = dbFactory;
            _logger = logger;
        }

        [Function("AccessorGetAllData")]
        public async Task<HttpResponseData> GetAll(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "accessor/data")]
            HttpRequestData req)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);

        }

        [Function("AccessorGetDataById")]
        public async Task<HttpResponseData> GetById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "accessor/data/{id}")]
            HttpRequestData req,
            string id)
        {
                return req.CreateResponse(HttpStatusCode.BadRequest);

        }
    }
}
