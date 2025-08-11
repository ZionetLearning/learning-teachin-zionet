using System.Threading;
using System.Threading.Tasks;
using Manager.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Manager.UnitTests.Messaging;

// A fake message type for tests
public record TestMsg(int Id);

public class QueueProcessorTests
{
    // Expose ExecuteAsync for testing
    private sealed class TestableQueueProcessor : QueueProcessor<TestMsg>
    {
        public TestableQueueProcessor(IQueueListener<TestMsg> listener, IServiceScopeFactory scopeFactory)
            : base(listener, scopeFactory) { }

        public Task RunAsync(CancellationToken token) => base.ExecuteAsync(token);
    }

    // A capturable listener to intercept the handler
    private sealed class CapturingListener : IQueueListener<TestMsg>
    {
        public Func<TestMsg, Func<Task>, CancellationToken, Task>? CapturedHandler;
        public CancellationToken CapturedToken;

        public Task StartAsync(Func<TestMsg, Func<Task>, CancellationToken, Task> handler, CancellationToken cancellationToken)
        {
            CapturedHandler = handler;
            CapturedToken = cancellationToken;
            // Don't run anything here; tests will invoke CapturedHandler manually.
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Processor_Resolves_Handler_From_Scope_And_Invokes_It()
    {
        // Arrange: mock handler that must be called with our message + renewLock
        var handlerMock = new Mock<IQueueHandler<TestMsg>>(MockBehavior.Strict);
        handlerMock.Setup(h => h.HandleAsync(
                It.Is<TestMsg>(m => m.Id == 42),
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // DI scope that returns our handler
        var provider = new ServiceCollection()
            .AddScoped<IQueueHandler<TestMsg>>(_ => handlerMock.Object)
            .BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var listener = new CapturingListener();
        var processor = new TestableQueueProcessor(listener, scopeFactory);

        using var cts = new CancellationTokenSource();

        // Act: start the processor (wires StartAsync and captures the handler)
        await processor.RunAsync(cts.Token);

        // Simulate an incoming message by invoking the captured handler
        Assert.NotNull(listener.CapturedHandler);
        await listener.CapturedHandler!.Invoke(new TestMsg(42), () => Task.CompletedTask, cts.Token);

        // Assert: our handler was called
        handlerMock.VerifyAll();
    }
}
