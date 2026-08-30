[![](https://img.shields.io/nuget/v/soenneker.extensions.valuetask.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.valuetask/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.valuetask/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.valuetask/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.valuetask.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.valuetask/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.valuetask/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.valuetask/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.ValueTask
Helpers for context-free awaits, synchronous bridges, and observed fire-and-forget `ValueTask` operations.

## Installation

```bash
dotnet add package Soenneker.Extensions.ValueTask
```

## Avoid context capture

```csharp
using Soenneker.Extensions.ValueTask;

Result result = await GetResultAsync().NoSync();
```

`NoSync()` is shorthand for `ConfigureAwait(false)` for both `ValueTask` and `ValueTask<T>`. It prevents the await from requesting the current synchronization context; it does not guarantee a different continuation thread.

## Synchronous bridges

```csharp
Result result = GetResultAsync().AwaitSync();
```

`AwaitSync()` blocks the current thread and unwraps the original exception. `AwaitSyncSafe()` registers a context-free continuation before blocking, which avoids the common deadlock caused solely by resuming that await on the blocked context. It still blocks a thread and cannot make an underlying operation safe if that operation itself requires the blocked context. Prefer normal `await`.

The cancellation token passed to `AwaitSyncSafe()` cancels only the synchronous wait. It does not cancel the underlying `ValueTask`; that operation can continue after the caller receives `OperationCanceledException`. Pass cancellation into the operation itself when it must stop.

## Observe detached work

```csharp
PublishMetricAsync().FireAndForgetSafe(exception =>
    logger.LogError(exception, "Metric publishing failed"));
```

`FireAndForgetSafe()` consumes the result or exception once and optionally invokes a synchronous callback for faults and cancellation. Callback failures are swallowed so a detached diagnostic cannot become an unhandled continuation failure.

Fire-and-forget work is not durable: it does not keep the process alive, retry, or guarantee delivery. Use a background queue for important operations.

## ValueTask rules still apply

These helpers consume the supplied `ValueTask`. Unless its source explicitly permits otherwise, do not await it again, call `AsTask()` afterward, or invoke more than one of these helpers on the same instance. Store a `Task` instead when an operation must support multiple consumers.
