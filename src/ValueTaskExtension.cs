using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Soenneker.Extensions.ValueTask;

/// <summary>
/// A collection of helpful ValueTask extension methods
/// </summary>
public static class ValueTaskExtension
{
    /// <summary>
    /// Configures an awaiter used to await this <see cref="ValueTask"/> to continue on a different context.
    /// Equivalent to <code>valueTask.ConfigureAwait(false);</code>.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to configure.</param>
    /// <returns>A configured awaitable.</returns>
    public static ConfiguredValueTaskAwaitable NoSync(this System.Threading.Tasks.ValueTask valueTask)
    {
        return valueTask.ConfigureAwait(false);
    }

    /// <summary>
    /// Configures an awaiter used to await this <see cref="ValueTask{T}"/> to continue on a different context.
    /// Equivalent to <code>valueTask.ConfigureAwait(false);</code>.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by this <see cref="ValueTask{T}"/>.</typeparam>
    /// <param name="valueTask">The <see cref="ValueTask{T}"/> to configure.</param>
    /// <returns>A configured awaitable.</returns>
    public static ConfiguredValueTaskAwaitable<T> NoSync<T>(this ValueTask<T> valueTask)
    {
        return valueTask.ConfigureAwait(false);
    }
}