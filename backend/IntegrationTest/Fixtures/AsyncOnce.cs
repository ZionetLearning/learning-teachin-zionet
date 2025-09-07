using System.Runtime.CompilerServices;

namespace IntegrationTests.Fixtures
{
    public static class AsyncOnce
    {
        private static readonly ConditionalWeakTable<string, OnceGate> _gates = new();

        private sealed class OnceGate
        {
            private readonly SemaphoreSlim _gate = new(1, 1);
            private volatile Task? _initTask;

            public async Task EnsureAsync(Func<Task> init)
            {
                if (_initTask is Task done)
                {
                    await done.ConfigureAwait(false);
                    return;
                }

                await _gate.WaitAsync().ConfigureAwait(false);
                try
                {
                    if (_initTask is null)
                        _initTask = Task.Run(init);

                    await _initTask.ConfigureAwait(false);
                }
                finally
                {
                    _gate.Release();
                }
            }
        }
        public static Task EnsureAsync(string key, Func<Task> init)
    => _gates.GetValue(key, _ => new OnceGate()).EnsureAsync(init);

    }
}
