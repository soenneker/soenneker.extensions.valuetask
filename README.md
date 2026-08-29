[![](https://img.shields.io/nuget/v/soenneker.extensions.valuetask.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.valuetask/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.valuetask/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.valuetask/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.extensions.valuetask.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.extensions.valuetask/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.extensions.valuetask/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.extensions.valuetask/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Extensions.ValueTask
A collection of helpful ValueTask extension methods.

## Installation

```bash
dotnet add package Soenneker.Extensions.ValueTask
```

## Quick start

```csharp
using Soenneker.Extensions.ValueTask;

// Given an existing System.Threading.Tasks.ValueTask named valueTask:
var result = valueTask.NoSync();
```

## Common operations

- `NoSync()` - Configures an awaiter for the specified `ValueTask` that does not capture the current synchronization context. Equivalent to calling `ConfigureAwait(false)`.
- `AwaitSync()` - Synchronously blocks until `ValueTask<T>` completes and returns its result. It can deadlock on a captured UI or ASP.NET synchronization context; prefer normal `await` or `AwaitSyncSafe()` when possible.
- `AwaitSyncSafe()` - Synchronously waits for a `ValueTask` to complete while avoiding synchronization-context deadlocks.
- `FireAndForgetSafe()` - Executes the specified `ValueTask` in a fire-and-forget manner, optionally invoking a callback if an exception occurs. This method ensures that exceptions are always observed to prevent unobserved-task exceptions.
