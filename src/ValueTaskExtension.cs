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
        return valueTask.GetAwaiter()
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
        valueTask.GetAwaiter()
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AwaitSyncSafe(this System.Threading.Tasks.ValueTask valueTask, CancellationToken cancellationToken = default)
    {
        if (valueTask.IsCompleted)
        {
            valueTask.GetAwaiter()
                     .GetResult(); // observe (cheap)
            return;
        }

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AwaitSyncSafe<T>(this ValueTask<T> valueTask, CancellationToken cancellationToken = default)
    {
        if (valueTask.IsCompleted)
            return valueTask.Result;

        return Task.Run(async () => await valueTask.NoSync(), cancellationToken)
                   .GetAwaiter()
                   .GetResult();
    }

    /// <summary>
    /// Executes the specified ValueTask in a fire-and-forget manner, optionally invoking a callback if an exception
    /// occurs.
    /// </summary>
    /// <remarks>Use this method to start a ValueTask without awaiting it, such as for background operations
    /// where the result is not needed. Exceptions thrown by the ValueTask are not propagated; instead, they are passed
    /// to the onException callback if provided. This method should be used with care, as unhandled exceptions may be
    /// silently ignored if no callback is specified.</remarks>
    /// <param name="valueTask">The ValueTask to execute asynchronously without awaiting its completion.</param>
    /// <param name="onException">An optional callback to invoke if the ValueTask completes with an exception. The callback receives the base
    /// exception as its argument. If null, exceptions are ignored.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FireAndForgetSafe(this System.Threading.Tasks.ValueTask valueTask, Action<Exception>? onException = null)
    {
        if (valueTask.IsCompletedSuccessfully)
            return;

        ValueTaskAwaiter awaiter = valueTask.GetAwaiter();

        // Completed (faulted/canceled) => observe synchronously, no allocations
        if (awaiter.IsCompleted)
        {
            try
            {
                awaiter.GetResult();
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }

            return;
        }

        // Not completed: must observe later (allocations are unavoidable here)
        Observe(valueTask, onException);
    }

    private static async void Observe(System.Threading.Tasks.ValueTask vt, Action<Exception>? handler)
    {
        try
        {
            await vt.NoSync();
        }
        catch (Exception ex)
        {
            handler?.Invoke(ex);
        }
    }

    /// <summary>
    /// Executes the specified ValueTask{T} in a fire-and-forget manner, optionally handling any exceptions that occur
    /// during its execution.
    /// </summary>
    /// <remarks>This method allows a ValueTask{T} to be executed asynchronously without awaiting its result.
    /// Use this method when the result of the task is not needed and exceptions should be handled via the provided
    /// callback or ignored. Exceptions that occur after the method returns are not propagated to the caller and must be
    /// handled by the onException callback if provided.</remarks>
    /// <typeparam name="T">The type of the result produced by the ValueTask.</typeparam>
    /// <param name="valueTask">The ValueTask{T} to execute without awaiting its completion.</param>
    /// <param name="onException">An optional callback that is invoked if the ValueTask{T} throws an exception. If null, exceptions are ignored.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FireAndForgetSafe<T>(this ValueTask<T> valueTask, Action<Exception>? onException = null)
    {
        if (valueTask.IsCompletedSuccessfully)
        {
            _ = valueTask.Result; // consume result
            return;
        }

        ValueTaskAwaiter<T> awaiter = valueTask.GetAwaiter();

        if (awaiter.IsCompleted)
        {
            try
            {
                _ = awaiter.GetResult();
            }
            catch (Exception ex)
            {
                onException?.Invoke(ex);
            }

            return;
        }

        // Not completed: must observe later (this will allocate; unavoidable).
        Observe(valueTask, onException);
    }

    private static async void Observe<T>(ValueTask<T> vt, Action<Exception>? handler)
    {
        try
        {
            _ = await vt.NoSync();
        }
        catch (Exception ex)
        {
            handler?.Invoke(ex);
        }
    }
}