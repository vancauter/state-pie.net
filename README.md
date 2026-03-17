# AtomicRx

AtomicRx is a reactive state container for .NET.

It is built around actions, reducers, stores, slices, and middleware. The library provides serialized dispatch, state
observation through `IObservable<IState>`, and reference-based change detection for immutable-by-convention state
snapshots.

## Contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [Quick Start](#quick-start)
- [Payload Actions](#payload-actions)
- [Slice Composition](#slice-composition)
- [Observing State](#observing-state)
- [Middleware](#middleware)
- [Thread-Safety Status](#thread-safety-status)
- [Current Limitations](#current-limitations)
- [Development](#development)
- [Contributing](#contributing)
- [License](#license)

## Features

AtomicRx provides:

- Plain actions with optional strongly typed payloads.
- Reducers for single-root or slice-composed state.
- A root store with immediate replay of the latest state to new subscribers.
- Logging middleware.

## Requirements

- .NET 10 SDK

## Installation

NuGet packaging is planned, but the package details are not finalized yet.

Placeholder:

```bash
dotnet add package AtomicRx
```

## Core Concepts

| Concept                         | Purpose                                                                    |
| ------------------------------- | -------------------------------------------------------------------------- |
| `IAction` / `IAction<TPayload>` | Immutable-by-convention messages dispatched to the store                   |
| `Reducer` / `Reducer<TState>`   | Functions that compute the next state from the current state and an action |
| `IStore`                        | Owns the current root state, dispatch pipeline, and state stream           |
| `ISlice`                        | Named slice definition used when composing a combined root reducer         |
| `AtomicMiddleware`              | Dispatch wrapper for diagnostics or custom interception                    |

Two conventions are important for correct behavior:

- Reducers should be pure.
- Reducers should return the existing state instance when nothing changed.

That second rule is what prevents unnecessary state publications.

## Quick Start

For simple scenarios, define a single root state and reducer directly.

```csharp
using AtomicRx;

public sealed record CounterState(int Count) : IState;

const string Increment = "@COUNTER/INCREMENT";
const string Decrement = "@COUNTER/DECREMENT";

static IState ReduceCounter(IState state, IAction action)
{
    var current = (CounterState)state;

    return action.Type switch
    {
        Increment => current with { Count = current.Count + 1 },
        Decrement => current with { Count = current.Count - 1 },
        _ => state
    };
}

using var store = Factory.CreateStore(new CounterState(0), ReduceCounter);

store.Dispatch(Factory.CreateAction(Increment));
store.Dispatch(Factory.CreateAction(Increment));

var current = (CounterState)store.State;
Console.WriteLine(current.Count);
```

## Payload Actions

Actions can also carry strongly typed payloads.

```csharp
using AtomicRx;

public sealed record PersonState(string Name) : IState;

static IState ReducePerson(IState state, IAction action)
{
    var current = (PersonState)state;

    return action.Type switch
    {
        "@PERSON/SET_NAME" when action is IAction<string> payloadAction
            => current with { Name = payloadAction.Payload },
        _ => state
    };
}

var action = Factory.CreateAction("@PERSON/SET_NAME", "Lee-Lee");
```

## Slice Composition

For multi-part state, define named slices and combine them into a single root reducer.

```csharp
using AtomicRx;
using System.Text.Json;

public sealed record CounterState(int Count) : IState;
public sealed record PersonState(string Name, int Age) : IState;

var counterSlice = Factory.CreateSlice(
    name: "counter",
    initialState: new CounterState(0),
    reducer: (state, action) => action.Type switch
    {
        "@COUNTER/INCREMENT" => state with { Count = state.Count + 1 },
        _ => state
    });

var personSlice = Factory.CreateSlice(
    name: "person",
    initialState: new PersonState("Guest", 30),
    reducer: (state, action) => action.Type switch
    {
        "@PERSON/ADD_AGE" => state with { Age = state.Age + 1 },
        "@PERSON/SET_NAME" when action is IAction<string> payloadAction
            => state with { Name = payloadAction.Payload },
        _ => state
    });

var (rootState, rootReducer) = Factory.CombineReducers(counterSlice, personSlice);

using var store = Factory.CreateStore(rootState, rootReducer);

store.Dispatch(Factory.CreateAction("@COUNTER/INCREMENT"));
store.Dispatch(Factory.CreateAction("@PERSON/SET_NAME", "Lee-Lee"));

// Combined state is currently easiest to inspect through observation or serialization.
Console.WriteLine(JsonSerializer.Serialize(store.State));
```

`Factory.CombineReducers` enforces unique slice names and preserves the existing root state reference when no slice
returns a new instance.

## Observing State

Each store exposes a replaying observable through `IStore.StateChanged`.

New subscribers immediately receive the latest state snapshot. The sequence completes when the store is disposed.

```csharp
using AtomicRx;

public sealed record CounterState(int Count) : IState;

static IState ReduceCounter(IState state, IAction action)
{
    var current = (CounterState)state;

    return action.Type switch
    {
        "@COUNTER/INCREMENT" => current with { Count = current.Count + 1 },
        _ => state
    };
}

using var store = Factory.CreateStore(new CounterState(0), ReduceCounter);

using var subscription = store.StateChanged.Subscribe(new Observer());

store.Dispatch(Factory.CreateAction("@COUNTER/INCREMENT"));

sealed class Observer : IObserver<IState>
{
    public void OnNext(IState value) => Console.WriteLine(((CounterState)value).Count);
    public void OnError(Exception error) => Console.WriteLine(error.Message);
    public void OnCompleted() => Console.WriteLine("Completed");
}
```

## Middleware

AtomicRx composes middleware around the dispatch pipeline in registration order.

The only built-in middleware today is logging.

```csharp
using var store = Factory.CreateStore(
    rootState,
    rootReducer,
    LoggingMiddleware.Create());
```

`LoggingMiddleware` writes each dispatched action type and suppresses logging failures so diagnostics cannot interrupt
dispatch.

Custom middleware can intercept, short-circuit, or forward actions through the `AtomicMiddleware` delegate.

## Thread-Safety Status

AtomicRx is being aligned with an explicit concurrency contract documented in
[docs/Threadsafe-Actions.md](docs/Threadsafe-Actions.md).

The current target contract is:

- `Dispatch` execution is serialized.
- `IStore.State` should provide strong cross-thread visibility.
- `StateChanged` should provide strict linearized ordering across replay, live updates, completion, and disposal.
- Observer callbacks are serialized per store stream, but there is no built-in scheduler or thread affinity.
- Middleware may perform nested dispatch, and nested actions participate in the total serialized dispatch order.
- Custom middleware may schedule background work, but that work is outside the original dispatch transaction.

That work is tracked in [docs/Threadsafe-Actions.md](docs/Threadsafe-Actions.md).

## Current Limitations

- Combined slice state is an internal runtime shape, so there is no public typed accessor for individual slices yet.
- The thread-safety contract is documented, but parts of it are still planned work rather than completed guarantees.
- The library does not currently ship higher-level async orchestration helpers.

## Development

Common repository commands from the root:

```bash
dotnet build *.sln
dotnet test tests/*/*.Tests.csproj
dotnet run --project dev/*/*.Dev.csproj
```

The repository also includes:

- A sample app under `dev/AtomicRx`
- Automated tests under `tests/AtomicRx`
- Ongoing design notes under `docs/`

## Contributing

Contributions should keep the library aligned with its current scope: explicit state transitions, predictable reducer
behavior, and a restrained built-in policy.

If you are changing concurrency behavior, update both the implementation and
[docs/Threadsafe-Actions.md](docs/Threadsafe-Actions.md).

## License

Licensed under the terms in `LICENSE`.
