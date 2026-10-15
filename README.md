# StatePie

[![CI](https://github.com/vancauter/state-pie.net/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/vancauter/state-pie.net/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/SC.StatePie)](https://www.nuget.org/packages/SC.StatePie)

StatePie is a lightweight .NET state-management library for immutable application state, built around a compact
reducer-based (sliced) store for predictable state transitions.

## Quick start

```csharp
using StatePie;

public interface ICounterAction : IAction;

public sealed record IncrementCounterAction : ICounterAction;
public sealed record SetCounterAction(int Value) : ICounterAction;
public sealed record CounterState(int Count) : IState;

var counterSlice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
    name: "counter",
    initialState: new CounterState(0),
    reducer: static (state, action) => action switch
    {
        IncrementCounterAction => state with { Count = state.Count + 1 },
        SetCounterAction set => state with { Count = set.Value },
        _ => state
    });

using var store = StoreFactory.CreateStore(counterSlice);

using var subscription = store.Subscribe(snapshot =>
{
    var counter = (CounterState)snapshot["counter"];
    Console.WriteLine($"Count: {counter.Count}");
});

store.Dispatch(new IncrementCounterAction());
store.Dispatch(new SetCounterAction(10));
```

## Design principles

- **Predictable One-Directional Data Flow:** State changes follow a single, clear path, making application behavior easy
  to trace and debug.
- **Single Source of Truth:** Application state is represented as a single, read-only root snapshot composed of distinct
  feature slices.
- **Strongly Typed & Fail-Fast:** Incompatible action types are caught at compile time, preventing runtime errors before
  they reach production.
- **Immutable Snapshots:** The root snapshot changes exclusively when a reducer returns a new state instance, ensuring
  deterministic state transitions.
- **Modular Feature Slices:** Slices encapsulate independent state and logic, promoting strong modularity and separation
  of concerns.

### State snapshots

- `StateSnapshot` is `IReadOnlyDictionary<string, IState>`
- each slice contributes an entry keyed by its name
- each reducer receives its slice's current state and the dispatched action
- the root snapshot is rebuilt only when a slice changes

### One action family per store

Each store is created for one action type, `TAction`, constrained to `IAction`.

```csharp
public interface IAppAction : IAction;
public sealed record IncrementCounterAction : IAppAction;
public sealed record AddTodoAction(string Text) : IAppAction;
```

The compiler rejects slices with incompatible action types.

### Multiple and nested slices

A store can compose multiple slices that share one action family. The root snapshot remains a flat dictionary; use
path-like slice names and nested state objects to represent hierarchical organization.

See the [multiple-slices](./examples/multiple-slices/) and [nested-slices](./examples/nested-slices/) examples.

### Reducers and no-op updates

Reducers are typed delegates:

```csharp
public delegate TState Reducer<TState, TAction>(TState state, TAction action)
    where TState : IState
    where TAction : class, IAction;
```

The store uses reference equality to detect no-op dispatches:

- returning the same state instance means the slice is unchanged
- if no slice changes, the root snapshot is reused
- subscribers are not notified

Reducers should return the existing state instance when an action does not change that slice.

### Subscriptions

The store supports root-snapshot and selected-value subscriptions.

#### Root snapshot subscription

```csharp
using var subscription = store.Subscribe(snapshot =>
{
    // inspect the whole snapshot
});
```

#### Selected value subscription

```csharp
var selector = StoreFactory.CreateSliceSelector<CounterState>("counter");

using var subscription = store.Subscribe(
    selector,
    counter => Console.WriteLine(counter.Count));
```

Selected-value subscriptions ignore unrelated slice changes. Provide a custom comparer to control how selected values
are compared.

See [selective-subscriptions](./examples/selective-subscriptions/) example.

### Middleware

StatePie supports synchronous middleware. Each middleware receives the current action and a `next` delegate that
continues the dispatch pipeline.

```csharp
using var store = StoreFactory.CreateStore(
    [counterSlice],
    (action, next) =>
    {
        Console.WriteLine($"before: {action}");
        next(action);
        Console.WriteLine("after");
    });
```

Middleware behavior:

- middleware runs in registration order, with the first registered middleware as the outermost layer
- each middleware may invoke `next` at most once per action
- invoking `next` more than once throws `InvalidOperationException`
- exceptions from reducers and listeners propagate without retrying the dispatch

Middleware is useful for cross-cutting concerns such as logging, validation, and instrumentation.

### Effects

Effects start asynchronous work after an action is committed and report the result by dispatching a typed action, so
reducers stay pure and synchronous.

```csharp
static async Task loadProfile(IProfileAction action, IEffectContext<IProfileAction> context)
{
    if (action is not LoadProfileAction load)
        return;

    var profile = await FetchProfileAsync(load.Id, context.CancellationToken);
    context.Dispatch(new ProfileLoadedAction(profile));
}

using var store = StoreFactory.CreateStore([profileSlice], [], [loadProfile]);
```

The second argument is the middleware list; pass `[]` when there is none.

Effect behavior:

- effects start in registration order after reducers commit and subscribers are notified; dispatch does not await them
- effects do not start for actions that middleware short-circuits or reducers reject, but they do start for no-op
  dispatches
- result actions are ordinary dispatches, so they pass through middleware and reducers and arrive in completion order
- the context's cancellation token is cancelled when the store is disposed, and result actions dispatched afterwards are
  dropped
- an exception thrown synchronously by an effect propagates from `Dispatch` after the state is committed
- a faulted effect task is passed to the optional `onEffectError` callback; without one the fault stays unobserved, and
  cancellation is never treated as a fault
- continuations resume wherever `await` resumes; StatePie does not capture or schedule synchronization contexts

Prefer middleware for synchronous cross-cutting concerns and effects for asynchronous work that returns results.

See [effects](./examples/effects/) example.

### Validation and guard rails

The library fails fast on invalid composition and runtime misuse, surfacing invalid setup at the boundary.

## Recommended usage patterns

StatePie is best suited to:

- application domains that prefer explicit state transitions
- desktop or client-side applications with a few well-defined slices
- teams that want a small, .NET-native reducer model similar to Flux or Redux
- scenarios where immutability and predictable observer behavior matter more than mutation

StatePie is less suited to:

- simple CRUD applications with mostly local, short-lived state, where plain objects or minimal MVVM state are
  sufficient
- high-frequency update loops, such as game loops or telemetry, where reducer fan-out and snapshot creation add
  undesirable overhead
- event-sourced systems that already use the event log as the source of truth and do not need an in-memory reducer store
- scenarios that require per-field reactive propagation rather than snapshot-based updates
