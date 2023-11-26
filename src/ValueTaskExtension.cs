using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Soenneker.Extensions.ValueTask;

/// <summary>
/// A collection of helpful ValueTask extension methods
/// </summary>
public static class ValueTaskExtension
{
    /// <summary>
    /// Equivalent to <code>valueTask.ConfigureAwait(false);</code>
    /// </summary>
    public static ConfiguredValueTaskAwaitable NoSync(this System.Threading.Tasks.ValueTask valueTask)
    {
        return valueTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Equivalent to <code>valueTask.ConfigureAwait(false);</code>
    /// </summary>
    public static ConfiguredValueTaskAwaitable<T> NoSync<T>(this ValueTask<T> valueTask)
    {
        return valueTask.ConfigureAwait(false);
    }
}