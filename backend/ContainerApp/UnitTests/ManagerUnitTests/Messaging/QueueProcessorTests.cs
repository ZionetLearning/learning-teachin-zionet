using Manager.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Manager.UnitTests.Messaging;

public record TestMsg(int Id);

public class QueueProcessorTests
{
    private sealed class TestableQueueProcessor : QueueProcessor<TestMsg>
    {
        public TestableQueueProcessor(IQueueListener<TestMsg> listener, IServiceScopeFactory scopeFactory)
            : base(listener, scopeFactory) { }

        public Task RunAsync(CancellationToken token) => base.ExecuteAsync(token);
    }

    private sealed class CapturingListener : IQueueListener<TestMsg>
    {
        public Func<TestMsg, Func<Task>, CancellationToken, Task>? CapturedHandler;
        public CancellationToken CapturedToken;

        public Task StartAsync(Func<TestMsg, Func<Task>, CancellationToken, Task> handler, CancellationToken cancellationToken)
        {
            CapturedHandler = handler;
            CapturedToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Processor_Resolves_Handler_From_Scope_And_Invokes_It()
    {
        var handlerMock = new Mock<IQueueHandler<TestMsg>>(MockBehavior.Strict);
        handlerMock.Setup(h => h.HandleAsync(
                It.Is<TestMsg>(m => m.Id == 42),
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

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
