using System.Net;
using Dapr.Client;
using Manager.Constants;
using Manager.Services.Clients.Accessor.Interfaces;

namespace Manager.Services.Clients.Accessor;

public class EmailAccessorClient : IEmailAccessorClient
{
    private readonly ILogger<EmailAccessorClient> _logger;
    private readonly DaprClient _daprClient;

    public EmailAccessorClient(ILogger<EmailAccessorClient> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    public async Task<IReadOnlyList<string>> GetRecipientEmailsByNameAsync(string name, CancellationToken ct = default)
    {
        _logger.LogInformation("Inside: {Method} in {Class}, Name={Name}", nameof(GetRecipientEmailsByNameAsync), nameof(EmailAccessorClient), name);
        try
        {
            var emails = await _daprClient.InvokeMethodAsync<List<string>>(
                HttpMethod.Get,
                AppIds.Accessor,
                $"emails-accessor/recipients/{name}",
                cancellationToken: ct
            );

            return emails ?? [];
        }
        catch (InvocationException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("No recipient emails found for name={Name}", name);
            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get recipient emails for name={Name}", name);
            throw;
        }
    }
}

