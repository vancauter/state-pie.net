# StatePie

StatePie is a lightweight, opinionated .NET state management library for immutable application state built around
slices, typed actions, and a single root snapshot.

The library is intentionally narrow in scope:

- each store is bound to a single action family
- each slice has a unique name and an initial state
- every dispatch runs through all slice reducers
- the root snapshot is replaced only when a slice returns a different state instance
- subscribers can listen to the full snapshot or to a selected value
- dispatch-time middleware can wrap the store pipeline in a deterministic order

This is not a general-purpose reactive framework; it is a compact reducer-based store for predictable state transitions.

## Core design

StatePie models state as a dictionary-based snapshot:

- `StateSnapshot` is `IReadOnlyDictionary<string, IState>`
- each slice contributes one entry under its name
- every slice reducer receives the current slice state and the action being dispatched
- the store rebuilds the root snapshot only when at least one slice changes

This is the runtime contract the library enforces:

- `IAction` marks a dispatchable action
- `IState` marks an immutable state object
- `StoreFactory.CreateSlice<TState, TAction>(...)` creates a named slice
- `StoreFactory.CreateStore(...)` composes one or more slices into a store
- `IStore<TAction>.Dispatch(...)` dispatches actions for that action family

## Installation

Add the project as a project reference in your solution, or reference the published package if one is available in your
build setup.

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

This example demonstrates the actual behavior of the library:

- a counter slice is created with a unique name
- the root state is a dictionary with one entry: `"counter"`
- dispatching actions updates the slice state
- the subscription is notified after the new snapshot is committed

## One action family per store

The generic action family is enforced at the type level. A store is created for a specific action type `TAction` where
`TAction : IAction`.

```csharp
public interface IAppAction : IAction;
public sealed record IncrementCounterAction : IAppAction;
public sealed record AddTodoAction(string Text) : IAppAction;
```

A single store may handle multiple slices, but all slices in that store must work with the same action family.

## Multiple slices

```csharp
using StatePie;

public interface IAppAction : IAction;

public sealed record IncrementCounterAction : IAppAction;
public sealed record AddTodoAction(string Text) : IAppAction;

public sealed record CounterState(int Count) : IState;
public sealed record TodosState(IReadOnlyList<string> Items) : IState;

var counterSlice = StoreFactory.CreateSlice<CounterState, IAppAction>(
    name: "counter",
    initialState: new CounterState(0),
    reducer: static (state, action) => action switch
    {
        IncrementCounterAction => state with { Count = state.Count + 1 },
        _ => state
    });

var todosSlice = StoreFactory.CreateSlice<TodosState, IAppAction>(
    name: "todos",
    initialState: new TodosState(Array.Empty<string>()),
    reducer: static (state, action) => action switch
    {
        AddTodoAction add => state with { Items = state.Items.Append(add.Text).ToArray() },
        _ => state
    });

using var store = StoreFactory.CreateStore(counterSlice, todosSlice);

using var subscription = store.Subscribe(snapshot =>
{
    var counter = (CounterState)snapshot["counter"];
    var todos = (TodosState)snapshot["todos"];
    Console.WriteLine($"Count: {counter.Count} | Todos: {todos.Items.Count}");
});

store.Dispatch(new IncrementCounterAction());
store.Dispatch(new AddTodoAction("Review pull request"));
```

The store runs all slice reducers for each action and merges the results into a single new root snapshot. A slice that
returns the same state instance is treated as a no-op, so the snapshot reference is preserved and no notification is
raised.

## Reducers and no-op updates

Reducers are typed delegates:

```csharp
public delegate TState Reducer<TState, TAction>(TState state, TAction action)
    where TState : IState
    where TAction : class, IAction;
```

The library decides whether a dispatch changed anything using reference equality:

- if a reducer returns the same object instance, the state is considered unchanged
- the root snapshot is reused
- subscriptions are not fired

This is an important behavior of the library and should be respected when designing reducers.

## Subscriptions

The store exposes two kinds of subscriptions.

### Root snapshot subscription

```csharp
using var subscription = store.Subscribe(snapshot =>
{
    // inspect the whole snapshot
});
```

### Selected value subscription

```csharp
var selector = StoreFactory.CreateSliceSelector<CounterState>("counter");

using var subscription = store.Subscribe(
    selector,
    counter => Console.WriteLine(counter.Count));
```

You can also transform the selected value with a projector:

```csharp
var selectedCount = StoreFactory.CreateSelector(
    StoreFactory.CreateSliceSelector<CounterState>("counter"),
    static counter => counter.Count);

using var subscription = store.Subscribe(selectedCount, count => Console.WriteLine(count));
```

The selected-value path is intentionally efficient: unrelated slice changes do not invoke the listener. A custom
comparer can be supplied to control equality semantics.

```csharp
using var subscription = store.Subscribe(
    StoreFactory.CreateSelector(
        StoreFactory.CreateSliceSelector<CounterState>("counter"),
        static counter => counter.Count),
    count => Console.WriteLine(count),
    EqualityComparer<int>.Default);
```

## Middleware

StatePie supports synchronous middleware. Each middleware receives the current action and a delegate to continue the
pipeline.

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

Important runtime behavior:

- middleware is executed in registration order
- the first registered middleware is the outermost layer
- `next` may be invoked at most once per action
- if middleware calls `next` more than once, the library throws `InvalidOperationException`
- exceptions thrown by the reducer or listener propagate without retrying the dispatch

This makes middleware suitable for cross-cutting concerns such as logging, validation, or instrumentation.

## StateMerger

StatePie also includes `StateMerger`, which produces a new state instance by copying current values and then overlaying
matching property values from an updates object.

```csharp
public sealed record CounterState(int Count, bool IsDirty) : IState;

var current = new CounterState(5, false);
var next = StateMerger.Merge(current, new { Count = 10 });
```

The actual behavior is straightforward:

- it creates a new instance of `TState`
- copies all public writable properties from the current state
- copies matching public properties from the updates object last
- properties with matching names and compatible types override the source state
- properties omitted from the updates object remain unchanged

This is a useful helper when you want a pragmatic immutable update without writing a custom reducer for every small
change.

## Validation and guard rails

The library fails fast on invalid composition and runtime misuse. Common guard clauses include:

- slice names must be non-empty
- slice names must be unique within a store
- a store must contain at least one slice
- a reducer cannot be null
- null actions are rejected
- null middleware entries are rejected
- dispatch after disposal throws `ObjectDisposedException`

This keeps invalid setup visible at the boundary rather than failing later in a less obvious place.

## Recommended usage patterns

StatePie is best suited to:

- application domains that prefer explicit state transitions
- desktop or client-side application state with a few well-defined slices
- teams that want a reducer model similar to Flux/Redux, but in a small .NET-native surface area
- cases where immutability and predictable observer logic matter more than object-mutating convenience

It is less suited to:

- Simple CRUD apps with mostly local, short-lived state where plain objects or minimal MVVM state is enough.
- Ultra-high-frequency update loops (ea. game loops or telemetry at very high rates) where reducers fan-out and snapshot
  creation overhead is probably undesirable.
- Event-sourced systems that already treat the event log as source of truth and do not need an in-memory reducer store
  abstraction.
- Scenarios requiring fine-grained reactive graph propagation per field instead of snapshot-based store updates.

## Summary

StatePie is a minimal yet strict state container:

- one action family per store
- named immutable slices
- reducer-driven state transitions
- root snapshot updates only when state actually changes
- selected subscriptions for efficient observation
- sync middleware for orchestration
- fast fail checks for invalid composition

If you prefer predictable application state over ad hoc mutation, StatePie gives you a compact and explicit model that
stays close to the actual runtime behavior of the library.
