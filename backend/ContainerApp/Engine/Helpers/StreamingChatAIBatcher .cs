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
    private readonly ILogger<StreamingChatAIBatcher> _logger;
    private bool _disposed;

    public StreamingChatAIBatcher(
        int minChars,
        TimeSpan maxLatency,
        Func<string, EngineChatStreamResponse> makeChunk,
        Func<EngineChatStreamResponse, Task> sendAsync,
        Func<ChatAiStreamDelta, EngineChatStreamResponse> makeToolChunk,
        Func<ChatAiStreamDelta, EngineChatStreamResponse> makeToolResultChunk,
        ILogger<StreamingChatAIBatcher> logger,
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
        _logger = logger;

        _logger.LogInformation("StreamingChatAIBatcher created. minChars={MinChars}, maxLatency={MaxLatency}ms", _minChars, _maxLatency.TotalMilliseconds);

        _loopTask = Task.Run(() => LoopAsync(_internalCts.Token), _internalCts.Token);
    }

    public async Task HandleUpdateAsync(ChatAiStreamDelta upd)
    {
        ArgumentNullException.ThrowIfNull(upd);

        try
        {
            if (!string.IsNullOrEmpty(upd.Delta) && upd.Stage == ChatStreamStage.Model)
            {
                _logger.LogTrace("HandleUpdateAsync: Model delta received. requestId={RequestId}, seq={Seq}, len={Len}", upd.RequestId, upd.Sequence, upd.Delta!.Length);
                await AddAsync(upd.Delta);
            }
            else if (upd.Stage == ChatStreamStage.Tool && !string.IsNullOrEmpty(upd.ToolCall))
            {
                _logger.LogTrace("HandleUpdateAsync: Tool call received. requestId={RequestId}, seq={Seq}", upd.RequestId, upd.Sequence);
                await FlushAsync();
                var toolChunk = _makeToolChunk(upd);
                await _sendAsync(toolChunk);
            }
            else if (upd.Stage == ChatStreamStage.ToolResult && !string.IsNullOrEmpty(upd.ToolResult))
            {
                _logger.LogTrace("HandleUpdateAsync: Tool result received. requestId={RequestId}, seq={Seq}", upd.RequestId, upd.Sequence);
                await FlushAsync();
                var toolResChunk = _makeToolResultChunk(upd);
                await _sendAsync(toolResChunk);
            }
            else
            {
                _logger.LogTrace("HandleUpdateAsync: Ignored update. requestId={RequestId}, stage={Stage}, seq={Seq}", upd.RequestId, upd.Stage, upd.Sequence);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("HandleUpdateAsync canceled. requestId={RequestId}, stage={Stage}, seq={Seq}", upd.RequestId, upd.Stage, upd.Sequence);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HandleUpdateAsync failed. requestId={RequestId}, stage={Stage}, seq={Seq}", upd.RequestId, upd.Stage, upd.Sequence);
            throw;
        }
    }

    public async Task AddAsync(string delta)
    {
        if (string.IsNullOrEmpty(delta) || _disposed)
        {
            _logger.LogTrace("AddAsync skipped. disposed={Disposed}, emptyDelta={Empty}", _disposed, string.IsNullOrEmpty(delta));
            return;
        }

        await _gate.WaitAsync(_ct).ConfigureAwait(false);
        try
        {
            _buffer.Append(delta);
            _logger.LogTrace("AddAsync: Appended delta. bufferLen={BufferLen}, minChars={MinChars}", _buffer.Length, _minChars);
            if (_buffer.Length >= _minChars)
            {
                await FlushCoreAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddAsync failed while appending or flushing.");
            throw;
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
            _logger.LogTrace("FlushAsync: Requested flush. bufferLen={BufferLen}", _buffer.Length);
            await FlushCoreAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FlushAsync failed.");
            throw;
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
                    _logger.LogTrace("LoopAsync: Disposed. Exiting loop.");
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
                            _logger.LogTrace("LoopAsync: Latency threshold reached. Flushing. bufferLen={BufferLen}, elapsedMs={Elapsed}", _buffer.Length, _sinceLastSend.ElapsedMilliseconds);
                            await FlushCoreAsync();
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "LoopAsync flush failed.");
                        throw;
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
            _logger.LogDebug("LoopAsync canceled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoopAsync encountered an error.");
            throw;
        }
    }

    private async Task FlushCoreAsync()
    {
        if (_buffer.Length == 0)
        {
            _logger.LogTrace("FlushCoreAsync: Nothing to flush.");
            return;
        }

        try
        {
            var text = _buffer.ToString();
            _buffer.Clear();
            _sinceLastSend.Restart();

            _logger.LogTrace("FlushCoreAsync: Sending chunk. len={Len}", text.Length);
            var chunk = _makeChunk(text);
            await _sendAsync(chunk).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FlushCoreAsync failed.");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _logger.LogInformation("Disposing StreamingChatAIBatcher.");

        await _internalCts.CancelAsync();

        try
        {
            await _loopTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("DisposeAsync: Loop task canceled.");
        }

        try
        {
            await _gate.WaitAsync(CancellationToken.None).ConfigureAwait(false);
            try
            {
                await FlushCoreAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DisposeAsync: Final flush failed.");
            }
            finally
            {
                _gate.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DisposeAsync: Failed to acquire gate for final flush.");
        }

        _timer.Dispose();
        _gate.Dispose();
        _internalCts.Dispose();

        _logger.LogInformation("StreamingChatAIBatcher disposed.");
    }
}