using Polly;

namespace Accessor.Services;

public interface IRetryPolicy
{
    IAsyncPolicy<HttpResponseMessage> CreateHttpPolicy(ILogger logger);
}
