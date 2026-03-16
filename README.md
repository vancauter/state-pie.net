# AtomicRx

AtomicRx is a minimal reactive state container for .NET that combines Redux-style dispatch, replayable state
observation, and reference-based change detection.

It is designed for applications that want explicit state transitions, predictable reducer behavior, and a small API
surface without a heavy framework around it.

## Contents

- [Why AtomicRx](#why-atomicrx)
- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Core concepts](#core-concepts)
- [Quick start](#quick-start)
- [Slice-based composition](#slice-based-composition)
- [Middleware](#middleware)
- [Memoized selectors](#memoized-selectors)
- [State observation and serialization](#state-observation-and-serialization)
- [Development](#development)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)

## Why AtomicRx

AtomicRx is aimed at the space between ad-hoc mutable state and a large application framework.

Use it when you want:

- Explicit state transitions through dispatched actions.
- Immutable-by-convention state snapshots.
- Immediate replay of the latest state to new subscribers.
- Reference-based change detection instead of deep comparison.
- Thread-safe dispatch and state publication.
- Optional middleware for dispatch interception.

## Features

- Small public API built around actions, reducers, stores, slices, middleware, and selectors.
- Immediate replay semantics through `IObservable<IState>` subscriptions.
- Serialized dispatch to keep reducer execution and state publication ordered.
- Single-root or multi-slice store composition.
- Reference-preserving reducer flow so unchanged state does not trigger noise.
- Memoized selector helpers for derived values.
- JSON serialization helper for state inspection and diagnostics.
- Reducer hot-swapping support for runtime replacement scenarios.

## Requirements

- .NET 10 SDK
- C# with nullable reference types enabled is recommended

The library currently targets `net10.0`.

## Installation

The repository is currently structured for direct source consumption.

1. Clone the repository.
2. Add a project reference to `src/AtomicRx/AtomicRx.csproj`.

```bash
dotnet add <your-project>.csproj reference ./src/AtomicRx/AtomicRx.csproj
```

If you only want to explore the library behavior, the repository also includes:

- A sample app in `dev/AtomicRx`
- Automated tests in `tests/AtomicRx`

## Core concepts

AtomicRx revolves around a small set of contracts:

| Concept                         | Purpose                                                                    |
| ------------------------------- | -------------------------------------------------------------------------- |
| `IAction` / `IAction<TPayload>` | Immutable messages dispatched to the store                                 |
| `Reducer` / `Reducer<TState>`   | Functions that compute the next state from the current state and an action |
| `IStore`                        | Holds the current state and exposes a replaying state stream               |
| `ISlice`                        | Named slice definition used for composed stores                            |
| `Middleware`                    | Dispatch pipeline wrapper for logging, thunks, or custom interception      |

Two conventions matter for correct behavior:

- Reducers should be pure.
- Reducers should return the existing state instance when nothing changed.

That second rule is how AtomicRx avoids unnecessary notifications.

## Quick start

For simple scenarios, create a store directly from a single root state and reducer.

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

### Creating payload actions

```csharp
var setName = Factory.CreateAction("@PERSON/SET_NAME", "Lee-Lee");
```

Reducers can branch on both `action.Type` and payload shape:

```csharp
static PersonState ReducePerson(PersonState state, IAction action)
{
	return action.Type switch
	{
		"@PERSON/SET_NAME" when action is IAction<string> payloadAction
			=> state with { Name = payloadAction.Payload },
		_ => state
	};
}
```

## Slice-based composition

For larger state graphs, define slices and combine them into a single root reducer.

```csharp
using AtomicRx;

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

Console.WriteLine(StateJson.Serialize(store.State));
```

`CombineReducers` enforces unique slice names and preserves the current root state instance when no slice changes.

## Middleware

AtomicRx middleware wraps the dispatch pipeline in registration order.

### Logging middleware

```csharp
using var store = Factory.CreateStore(
	rootState,
	rootReducer,
	LoggingMiddleware.Create());
```

The built-in logging middleware writes each dispatched action type and deliberately avoids interrupting dispatch if
logging fails.

### Thunk middleware

The built-in thunk middleware supports these payload shapes:

- `Action<IStore>`
- `Action<Dispatch, IStore>`
- `Func<IStore, Task>`
- `Func<Dispatch, IStore, Task>`

Example:

```csharp
using var store = Factory.CreateStore(
	rootState,
	rootReducer,
	ThunkMiddleware.Create());

var thunk = new Func<IStore, Task>(async currentStore =>
{
	currentStore.Dispatch(Factory.CreateAction("@COUNTER/INCREMENT"));
	await Task.Delay(10);
	currentStore.Dispatch(Factory.CreateAction("@COUNTER/INCREMENT"));
});

store.Dispatch(Factory.CreateAction("THUNK", thunk));
```

## Memoized selectors

Use selectors to cache derived values from strongly typed state.

```csharp
using AtomicRx;

public sealed record CounterState(int Count) : IState;

var doubleCount = Selectors.CreateMemoizedSelector(
	(CounterState state) => state.Count * 2);

var state = new CounterState(21);
var result = doubleCount(state);
```

`CreateMemoizedSelector` performs single-entry memoization. If consecutive inputs compare equal, the cached result is
reused.

## State observation and serialization

Every store exposes a replaying observable through `IStore.StateChanged`.

New subscribers receive the current state immediately, and the sequence completes when the store is disposed.

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

public sealed class StateObserver : IObserver<IState>
{
	public void OnNext(IState value)
	{
		Console.WriteLine(StateJson.Serialize(value));
	}

	public void OnError(Exception error)
	{
		Console.Error.WriteLine(error);
	}

	public void OnCompleted()
	{
		Console.WriteLine("Store completed.");
	}
}

using var store = Factory.CreateStore(new CounterState(0), ReduceCounter);
using var subscription = store.StateChanged.Subscribe(new StateObserver());

store.Dispatch(Factory.CreateAction("@COUNTER/INCREMENT"));
```

`StateJson.Serialize` is especially useful when you are using composed slice state and want a stable JSON view for
logging, debugging, or tests.

## Development

Build the solution:

```bash
dotnet build *.sln
```

Run the tests:

```bash
dotnet test tests/*/*.Tests.csproj
```

Run the sample app:

```bash
dotnet run --project dev/*/*.Dev.csproj
```

## Roadmap

Planned or parked follow-up work is tracked in [docs/FEATURES.md](docs/FEATURES.md).

Current roadmap themes include:

- Time-travel middleware
- Snapshot deserialization and migration support

## Contributing

Issues and pull requests are welcome.

When contributing:

- Keep the API surface small and intentional.
- Preserve reference-based change semantics.
- Add or update tests for behavior changes.
- Prefer focused changes over broad abstraction layers.

## License

This project is licensed under the terms in [LICENSE](LICENSE).
