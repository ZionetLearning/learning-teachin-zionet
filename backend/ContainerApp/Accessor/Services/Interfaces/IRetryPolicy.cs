using Polly;

namespace Accessor.Services.Interfaces;

public interface IRetryPolicy
{
    IAsyncPolicy<HttpResponseMessage> CreateHttpPolicy(ILogger logger);
}
