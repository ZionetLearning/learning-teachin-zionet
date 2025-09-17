using System.Diagnostics;
using System.Text;
using Engine.Models.Chat;

namespace Engine.Helpers;

public sealed class StreamingChatAIBatcher : IAsyncDisposable
{
    private readonly int _minChars;
    private readonly TimeSpan _maxLatency;
    private readonly Func<EngineChatStreamResponse, Task> _sendAsync;
    private readonly Func<string, EngineChatStreamResponse> _makeChunk;
    private readonly Func<ChatAiStreamDelta, EngineChatStreamResponse> _makeToolChunk;
    private readonly Func<ChatAiStreamDelta, EngineChatStreamResponse> _makeToolResultChunk;
    private readonly CancellationToken _ct;
    private readonly CancellationTokenSource _internalCts;
    private readonly Task _loopTask;
    private readonly StringBuilder _buffer = new();
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly PeriodicTimer _timer;
    private readonly Stopwatch _sinceLastSend = Stopwatch.StartNew();
    private bool _disposed;

    public StreamingChatAIBatcher(
        int minChars,
        TimeSpan maxLatency,
        Func<string, EngineChatStreamResponse> makeChunk,
        Func<EngineChatStreamResponse, Task> sendAsync,
        Func<ChatAiStreamDelta, EngineChatStreamResponse> makeToolChunk,
        Func<ChatAiStreamDelta, EngineChatStreamResponse> makeToolResultChunk,
        CancellationToken ct)
    {
        _minChars = Math.Max(1, minChars);
        _maxLatency = maxLatency <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(250) : maxLatency;
        _makeChunk = makeChunk ?? throw new ArgumentNullException(nameof(makeChunk));
        _sendAsync = sendAsync ?? throw new ArgumentNullException(nameof(sendAsync));
        _makeToolChunk = makeToolChunk ?? throw new ArgumentNullException(nameof(makeToolChunk));
        _makeToolResultChunk = makeToolResultChunk ?? throw new ArgumentNullException(nameof(makeToolResultChunk));
        _ct = ct;
        _internalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));
        _loopTask = Task.Run(() => LoopAsync(_internalCts.Token), _internalCts.Token);
    }

    public async Task HandleUpdateAsync(ChatAiStreamDelta upd)
    {
        if (!string.IsNullOrEmpty(upd.Delta) && upd.Stage == ChatStreamStage.Model)
        {
            await AddAsync(upd.Delta);
        }
        else if (upd.Stage == ChatStreamStage.Tool && !string.IsNullOrEmpty(upd.ToolCall))
        {
            await FlushAsync();
            var toolChunk = _makeToolChunk(upd);
            await _sendAsync(toolChunk);
        }
        else if (upd.Stage == ChatStreamStage.ToolResult && !string.IsNullOrEmpty(upd.ToolResult))
        {
            await FlushAsync();
            var toolResChunk = _makeToolResultChunk(upd);
            await _sendAsync(toolResChunk);
        }
    }

    public async Task AddAsync(string delta)
    {
        if (string.IsNullOrEmpty(delta) || _disposed)
        {
            return;
        }

        await _gate.WaitAsync(_ct).ConfigureAwait(false);
        try
        {
            _buffer.Append(delta);
            if (_buffer.Length >= _minChars)
            {
                await FlushCoreAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task FlushAsync()
    {
        await _gate.WaitAsync(_ct).ConfigureAwait(false);
        try
        {
            await FlushCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task LoopAsync(CancellationToken token)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(token).ConfigureAwait(false))
            {
                if (_disposed)
                {
                    break;
                }

                if (_buffer.Length == 0)
                {
                    continue;
                }

                if (_sinceLastSend.Elapsed >= _maxLatency)
                {
                    await _gate.WaitAsync(token);
                    try
                    {
                        if (_buffer.Length > 0 && _sinceLastSend.Elapsed >= _maxLatency)
                        {
                            await FlushCoreAsync();
                        }
                    }
                    finally
                    {
                        _gate.Release();
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {

        }
    }

    private async Task FlushCoreAsync()
    {
        if (_buffer.Length == 0)
        {
            return;
        }

        var text = _buffer.ToString();
        _buffer.Clear();
        _sinceLastSend.Restart();

        var chunk = _makeChunk(text);
        await _sendAsync(chunk).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await _internalCts.CancelAsync();

        try
        {
            await _loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }

        try
        {
            await _gate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
            try
            {
                await FlushCoreAsync().ConfigureAwait(false);
            }
            finally
            {
                _gate.Release();
            }
        }
        catch { }

        _timer.Dispose();
        _gate.Dispose();
        _internalCts.Dispose();
    }
}