using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Net;
using System.Text;

namespace Manager.Functions;

public class RbbitProducerFunction
{
    private readonly ILogger<RbbitProducerFunction> _logger;

    public RbbitProducerFunction(ILogger<RbbitProducerFunction> logger)
    {
        _logger = logger;
    }

    [Function("SendMessage")]
    public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "send")] HttpRequestData req)
    {
        var message = await req.ReadAsStringAsync();

        var factory = new ConnectionFactory
        {
            Uri = new Uri(Environment.GetEnvironmentVariable("RabbitMQConnection"))
        };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync("myqueue", false, false, false, null);
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync("", "myqueue", body: body);



        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Message sent to RabbitMQ!");
        return response;
    }
}