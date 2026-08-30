using System;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.ValueTask.Utils;

internal sealed class ObserveState<T>
{
    private ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _awaiter;
    private readonly Action<Exception>? _handler;

    public ObserveState(ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter, Action<Exception>? handler)
    {
        _awaiter = awaiter;
        _handler = handler;
    }

    public void Continue()
    {
        try
        {
            _ = _awaiter.GetResult();
        }
        catch (Exception ex)
        {
            try
            {
                _handler?.Invoke(ex);
            }
            catch
            {
            }
        }
    }
}
