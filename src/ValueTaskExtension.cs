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

    /// <summary>
    /// Synchronously runs the specified <see cref="ValueTask{TResult}"/> and returns its result.
    /// </summary>
    /// <typeparam name="T">The result type of the <see cref="ValueTask{T}"/>.</typeparam>
    /// <param name="valueTask">The <see cref="ValueTask{T}"/> to run synchronously.</param>
    /// <returns>The result of the completed <see cref="ValueTask{T}"/>.</returns>
    /// <remarks>
    /// If the <see cref="ValueTask{T}"/> has not yet completed, this method will block the calling thread
    /// until it does. This may lead to deadlocks if called on a context that does not allow synchronous blocking.
    /// </remarks>
    public static T RunSync<T>(this ValueTask<T> valueTask)
    {
        return valueTask.IsCompletedSuccessfully
            ? valueTask.Result
            : valueTask.AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Synchronously runs the specified <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to run synchronously.</param>
    /// <remarks>
    /// If the <see cref="ValueTask"/> has not yet completed, this method will block the calling thread
    /// until it does. This may lead to deadlocks if called on a context that does not allow synchronous blocking.
    /// </remarks>
    public static void RunSync(this System.Threading.Tasks.ValueTask valueTask)
    {
        if (!valueTask.IsCompletedSuccessfully)
            valueTask.AsTask().GetAwaiter().GetResult();
    }
}