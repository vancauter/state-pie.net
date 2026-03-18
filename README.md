# AtomicState

AtomicState is a reactive state container for .NET.

It is built around actions, reducers, stores, slices, and middleware. The library provides serialized dispatch, state
observation through `IObservable<IState>`, and reference-based change detection for immutable-by-convention state
snapshots.

## Contents

- [Status](#status)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Core Concepts](#core-concepts)
- [Quick Start](#quick-start)
- [Payload Actions](#payload-actions)
- [Slice Composition](#slice-composition)
- [Observing State](#observing-state)
- [Middleware](#middleware)
- [Architecture Notes](#architecture-notes)
- [Thread-Safety Status](#thread-safety-status)
- [Current Limitations](#current-limitations)
- [Development](#development)

## Status

AtomicState is currently pre-release.

Breaking changes are still acceptable when they materially improve the API, naming, or structure. The package story is
not finalized yet, so this repository is currently the source of truth for usage and development guidance.

## Features

AtomicState provides:

- Plain actions with optional strongly typed payloads.
- Reducers for single-root or slice-composed state.
- A root store with immediate replay of the latest state to new subscribers.
- Logging middleware.

## Requirements

- .NET 10 SDK

## Installation

NuGet packaging is planned, but the package details are not finalized yet.

For now, consume AtomicState from a local clone with a project reference:

```bash
dotnet add <your-project>.csproj reference src/AtomicState/AtomicState.csproj
```

## Core Concepts

| Concept                         | Purpose                                                                    |
| ------------------------------- | -------------------------------------------------------------------------- |
| `IAction` / `IAction<TPayload>` | Immutable-by-convention messages dispatched to the store                   |
| `Reducer` / `Reducer<TState>`   | Functions that compute the next state from the current state and an action |
| `IStore`                        | Owns the current root state, dispatch pipeline, and state stream           |
| `ISlice`                        | Named slice definition used when composing a combined root reducer         |
| `Middleware`                    | Dispatch wrapper for diagnostics or custom interception                    |

Two conventions are important for correct behavior:

- Reducers should be pure.
- Reducers should return the existing state instance when nothing changed.

That second rule is what prevents unnecessary state publications.

## Quick Start

For simple scenarios, define a single root state and reducer directly.

```csharp
using SC.AtomicState;

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
using SC.AtomicState;

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
using SC.AtomicState;

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

var counter = StateAccess.GetSliceState<CounterState>(store.State, "counter");
var person = StateAccess.GetSliceState<PersonState>(store.State, "person");

Console.WriteLine($"{counter.Count}: {person.Name} ({person.Age})");
```

`Factory.CombineReducers` enforces unique slice names and preserves the existing root state reference when no slice
returns a new instance.

Store creation performs an internal `@@INIT` dispatch before returning. If reducer or middleware logic throws during
that initialization pass, `Factory.CreateStore(...)` throws and no partially initialized store instance is returned.

## Observing State

Each store exposes a replaying observable through `IStore.StateChanged`.

New subscribers immediately receive the latest state snapshot. The intended concurrency contract is that subscription
replay, live updates, completion, and disposal are observed in one linearized order. Observer callbacks are serialized
per store stream, run on the calling thread, and have no built-in thread affinity or scheduler. The sequence completes
when the store is disposed.

The intended `IStore.State` contract is that cross-thread readers that are not synchronized observe the latest committed
root state.

```csharp
using SC.AtomicState;

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

AtomicState composes middleware around the dispatch pipeline in registration order.

The only built-in middleware today is logging.

```csharp
using var store = Factory.CreateStore(
    rootState,
    rootReducer,
    LoggingMiddleware.Create());
```

`LoggingMiddleware` writes each dispatched action type and suppresses logging failures so diagnostics cannot interrupt
dispatch.

Custom middleware can intercept, short-circuit, or forward actions through the `Middleware` delegate.

Middleware may call `Dispatch` again while a dispatch is already in progress. When it does, nested actions participate
in the same total serialized dispatch order as all other store work. Middleware may also schedule work onto background
execution contexts. Any later dispatch from that scheduled work is still serialized by the store, but it is outside the
original dispatch transaction.

## Architecture Notes

AtomicState stays deliberately small.

- The public surface is centered on actions, reducers, slices, stores, and middleware rather than higher-level
  opinionated workflows.
- `Factory.CreateStore(...)` owns the dispatch pipeline and runs an internal `@@INIT` action before the store is
  returned.
- `Factory.CombineReducers(...)` builds a combined root state from named slices while preserving root-state identity for
  logical no-op actions.
- The combined root-state shape remains internal. `StateAccess` is the supported way to read typed slice values from a
  combined store state.
- Middleware wraps dispatch in registration order and can observe or redirect action flow without taking ownership of
  state storage.

## Thread-Safety Status

AtomicState documents the following concurrency contract:

- `Dispatch` execution is serialized.
- `IStore.State` provides strong cross-thread visibility for the latest committed root state.
- `StateChanged` provides strict linearized ordering across subscription replay, live updates, completion, and disposal.
- Observer callbacks are serialized per store stream, run on the calling thread, and have no built-in thread affinity or
  scheduler.
- Nested dispatch triggered by middleware participates in the same total serialized dispatch order.
- Background work scheduled by middleware is outside the originating dispatch transaction, although any later dispatch
  from that work is still serialized by the store.

## Current Limitations

- Combined slice state stays internal, but `StateAccess` provides a supported way to read named typed slices from a root
  state produced by `Factory.CombineReducers(...)`.
- The library does not currently ship higher-level async orchestration helpers.

## Development

Common repository commands from the root:

```bash
dotnet build *.sln
dotnet test tests/*/*.Tests.csproj --filter "Category!=Perf"
dotnet test tests/*/*.Tests.csproj --filter "Category=Perf"
dotnet run --verbosity q --project dev/*/*.Dev.csproj
```

The repository also includes:

- A sample app under `dev/AtomicState`
- Automated tests under `tests/AtomicState`
- No additional user-facing docs under `docs/` yet; this README is currently the primary user-facing document.
