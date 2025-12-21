using System;
using System.Runtime.CompilerServices;
using System.Threading;
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AwaitSync<T>(this ValueTask<T> valueTask)
    {
        return valueTask.IsCompletedSuccessfully
            ? valueTask.Result
            : valueTask.AsTask()
                       .GetAwaiter()
                       .GetResult();
    }

    /// <summary>
    /// Synchronously runs the specified <see cref="ValueTask"/>.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to run synchronously.</param>
    /// <remarks>
    /// If the <see cref="ValueTask"/> has not yet completed, this method will block the calling thread
    /// until it does. This may lead to deadlocks if called on a context that does not allow synchronous blocking.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitSync(this System.Threading.Tasks.ValueTask valueTask)
    {
        if (!valueTask.IsCompletedSuccessfully)
            valueTask.AsTask()
                     .GetAwaiter()
                     .GetResult();
    }

    /// <summary>
    /// Synchronously waits for a <see cref="ValueTask"/> to complete in a safe manner,
    /// avoiding deadlocks by offloading the execution to a background thread and not capturing the synchronization context.
    /// </summary>
    /// <param name="valueTask">The <see cref="ValueTask"/> to wait for.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the background operation.</param>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the provided token.</exception>
    /// <exception cref="AggregateException">Thrown if the task faults; inner exceptions contain the actual errors.</exception>
    /// <remarks>
    /// This method should be used when asynchronous code must be waited on from synchronous contexts, such as in constructors or legacy APIs,
    /// without risking deadlocks commonly caused by synchronization context capture (e.g., UI threads or ASP.NET).
    /// </remarks>
    public static void AwaitSyncSafe(this System.Threading.Tasks.ValueTask valueTask, CancellationToken cancellationToken = default)
    {
        Task.Run(async () => await valueTask.NoSync(), cancellationToken)
            .GetAwaiter()
            .GetResult();
    }

    /// <summary>
    /// Synchronously waits for a <see cref="ValueTask{TResult}"/> to complete and returns its result in a safe manner,
    /// avoiding deadlocks by offloading the execution to a background thread and not capturing the synchronization context.
    /// </summary>
    /// <typeparam name="T">The result type of the <see cref="ValueTask{TResult}"/>.</typeparam>
    /// <param name="valueTask">The <see cref="ValueTask{TResult}"/> to wait for.</param>
    /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to cancel the background operation.</param>
    /// <returns>The result of the completed task.</returns>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the provided token.</exception>
    /// <exception cref="AggregateException">Thrown if the task faults; inner exceptions contain the actual errors.</exception>
    /// <remarks>
    /// This method is useful when you must synchronously retrieve the result of asynchronous code, such as in library code or integration with legacy systems,
    /// without risking deadlocks due to synchronization context capture.
    /// </remarks>
    public static T AwaitSyncSafe<T>(this ValueTask<T> valueTask, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () => await valueTask.NoSync(), cancellationToken)
                   .GetAwaiter()
                   .GetResult();
    }
}