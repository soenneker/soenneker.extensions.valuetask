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
    public static T AwaitSync<T>(this ValueTask<T> valueTask)
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
    public static void AwaitSync(this System.Threading.Tasks.ValueTask valueTask)
    {
        if (!valueTask.IsCompletedSuccessfully)
            valueTask.AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes an asynchronous <see cref="ValueTask"/> operation in a synchronous context by offloading it to a background thread,
    /// avoiding potential deadlocks that can occur when blocking on async code (e.g., on the UI thread).
    /// </summary>
    /// <param name="func">The asynchronous operation to execute.</param>
    /// <remarks>
    /// This method is useful in contexts where asynchronous code must be invoked synchronously (e.g., in constructors,
    /// event handlers, or system callbacks such as BroadcastReceivers). The operation is executed on the thread pool
    /// using <see cref="Task.Run(System.Action)"/>, which helps prevent common deadlock scenarios caused by
    /// synchronously waiting on async operations that capture a synchronization context.
    /// </remarks>
    /// <exception cref="AggregateException">Thrown if the task faults and throws an exception.</exception>
    public static void AwaitSyncSafe(this Func<System.Threading.Tasks.ValueTask> func)
    {
        Task.Run(async () => await func().NoSync()).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes an asynchronous <see cref="ValueTask{TResult}"/> operation in a synchronous context by offloading it to a background thread,
    /// avoiding potential deadlocks that can occur when blocking on async code (e.g., on the UI thread).
    /// </summary>
    /// <typeparam name="T">The type of result returned by the asynchronous operation.</typeparam>
    /// <param name="func">The asynchronous operation to execute, returning a <see cref="ValueTask{TResult}"/>.</param>
    /// <returns>The result of the asynchronous operation.</returns>
    /// <remarks>
    /// This method is useful in contexts where asynchronous code must be invoked synchronously (e.g., in constructors,
    /// event handlers, or system callbacks such as BroadcastReceivers). The operation is executed on the thread pool
    /// using <see cref="Task.Run(System.Func{Task{TResult}})"/>, which helps prevent common deadlock scenarios caused by
    /// synchronously waiting on async operations that capture a synchronization context.
    /// </remarks>
    /// <exception cref="AggregateException">Thrown if the task faults and throws an exception.</exception>
    public static T AwaitSyncSafe<T>(this Func<ValueTask<T>> func)
    {
        return Task.Run(async () => await func().NoSync()).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes an asynchronous <see cref="ValueTask"/> operation in a synchronous context by offloading it to a background thread,
    /// avoiding potential deadlocks that can occur when blocking on async code (e.g., on the UI thread).
    /// </summary>
    /// <param name="func">The asynchronous operation to execute, accepting a <see cref="CancellationToken"/>.</param>
    /// <param name="cancellationToken">An optional cancellation token to observe during execution.</param>
    /// <remarks>
    /// This method is useful in contexts where asynchronous code must be invoked synchronously (e.g., in constructors,
    /// event handlers, or system callbacks such as BroadcastReceivers). The operation is executed on the thread pool
    /// using <see cref="Task.Run(System.Action)"/>, which helps prevent common deadlock scenarios caused by
    /// synchronously waiting on async operations that capture a synchronization context.
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown if the task is canceled via the provided <paramref name="cancellationToken"/>.</exception>
    /// <exception cref="AggregateException">Thrown if the task faults and throws an exception.</exception>
    public static void AwaitSyncSafe(this Func<CancellationToken, System.Threading.Tasks.ValueTask> func, CancellationToken cancellationToken = default)
    {
        Task.Run(async () => await func(cancellationToken).NoSync(), cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes an asynchronous <see cref="ValueTask{TResult}"/> operation in a synchronous context by offloading it to a background thread,
    /// avoiding potential deadlocks that can occur when blocking on async code (e.g., on the UI thread).
    /// </summary>
    /// <typeparam name="T">The type of result returned by the asynchronous operation.</typeparam>
    /// <param name="func">The asynchronous operation to execute, accepting a <see cref="CancellationToken"/> and returning a <see cref="ValueTask{TResult}"/>.</param>
    /// <param name="cancellationToken">An optional cancellation token to observe during execution.</param>
    /// <returns>The result of the asynchronous operation.</returns>
    /// <remarks>
    /// This method is useful in contexts where asynchronous code must be invoked synchronously (e.g., in constructors,
    /// event handlers, or system callbacks such as BroadcastReceivers). The operation is executed on the thread pool
    /// using <see cref="Task.Run(System.Func{Task{TResult}})"/>, which helps prevent common deadlock scenarios caused by
    /// synchronously waiting on async operations that capture a synchronization context.
    /// </remarks>
    /// <exception cref="OperationCanceledException">Thrown if the task is canceled via the provided <paramref name="cancellationToken"/>.</exception>
    /// <exception cref="AggregateException">Thrown if the task faults and throws an exception.</exception>
    public static T AwaitSyncSafe<T>(this Func<CancellationToken, ValueTask<T>> func, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () => await func(cancellationToken).NoSync(), cancellationToken).GetAwaiter().GetResult();
    }
}