using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Soenneker.Extensions.ValueTask.Utils;

internal sealed class SyncWaitState<T>
{
    private readonly ManualResetEventSlim _mres = new(false);
    private ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter;
    private Exception? _exception;
    private T? _result;
    private int _completionState;

    public SyncWaitState(ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter)
    {
        _awaiter = awaiter;
        _awaiter.UnsafeOnCompleted(Continue);
    }

    private void Continue()
    {
        try
        {
            _result = _awaiter.GetResult();
        }
        catch (Exception ex)
        {
            _exception = ex;
        }
        finally
        {
            _mres.Set();

            if (Interlocked.CompareExchange(ref _completionState, 1, 0) == 2)
                _mres.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Wait(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
        {
            _mres.Wait();
            return;
        }

        try
        {
            _mres.Wait(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            if (Interlocked.CompareExchange(ref _completionState, 2, 0) == 1)
                _mres.Dispose();

            throw;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetResultOrThrow()
    {
        _mres.Dispose();

        if (_exception is not null)
            throw _exception;

        return _result!;
    }
}
