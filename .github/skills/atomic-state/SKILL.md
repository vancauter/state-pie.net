---
name: atomic-state
description: Reference and integration guide for the AtomicState reactive state container.
---

** Ignore this skill if you are working in the AtomicState workspace itself. Use existing source code instead **

s# Purpose

Concise reference for integrators and automation that need authoritative, copy-paste examples and the public contracts
of the AtomicState library. This SKILL is intended to help agents and humans integrate, test, and extend AtomicState
without reading the entire source tree.

# When to use

- Integrating deterministic application-level state (dashboards, orchestrators, registries).
- Writing reducers, middleware, persistence adapters, or testing harnesses for AtomicState.
- Generating examples, docs, or automation that control or observe AtomicState stores.

# Capabilities

- Deterministic reducer-driven state container with reference-based change detection.
- Composable slice model and `CombineReducers` to build a flat root snapshot.
- Serialized dispatch with middleware support and nested dispatch semantics.
- Persist adapters (pluggable `ISnapshotStore`) and optional time-travel middleware for undo/redo and replay.
- Lightweight telemetry hooks via `IStoreMetrics`.

# Public Contracts (summary)

- `IAction` / `IAction<TPayload>`: immutable message contract with `string Type` and optional `Payload`.

- `IState`: marker interface for state snapshot nodes used by slices and combined root state.

- `Reducer` / `Reducer<TState>`: delegate `(IState state, IAction action) => IState` (typed `TState` variant available).

- `SliceHandle<TState>`: typed handle returned by `Factory.CreateSlice` used for composition and state access.

- `Factory` (static): helpers including `CreateAction`, `CreateAction<T>`, `CreateSlice`, `CombineReducers`,
  `CreateStore` and overloads with middleware, metrics and options.

- `IStore`: main runtime surface exposing `IState State`, `IObservable<IState> StateChanged`, and
  `void Dispatch(IAction)`.

- `Middleware` / `IDispatchContext`: dispatch pipeline building blocks. Middleware is invoked in provided order and
  receives an `IDispatchContext` which allows nested `Dispatch` during a dispatch transaction.

- `ISnapshotStore`, `PersistMiddleware`, `InMemorySnapshotStore`, and `StoreSnapshotHelper`: persistence adapters and
  helpers for saving/restoring snapshots.

- `Telemetry.IStoreMetrics` / `Telemetry.NoOpStoreMetrics`: optional metrics hooks wired into store creation.

- `MiddlewareEx.TimeTravelMiddleware` / `ITimeTravelController`: optional time-travel support (history, undo/redo,
  replay controller).

- Helpers: `TypedActionId<T>`, `MiddlewareHelpers` (Safe async dispatch helpers), test seed helpers like
  `CreateSeededStore`.

# Short Examples

Basic store creation and dispatch:

```csharp
// rootState must implement IState
var (rootState, rootReducer) = Factory.CombineReducers(counterSlice, settingsSlice);
using var store = Factory.CreateStore(rootState, rootReducer);

store.Dispatch(Factory.CreateAction("APP/INIT"));
var snapshot = store.State; // latest committed IState
```

Typed action with payload:

```csharp
var action = Factory.CreateAction("USER/SET_NAME", new SetNamePayload("Alice"));
store.Dispatch(action);
```

Creating and combining slices:

```csharp
var counterSlice = Factory.CreateSlice("counter", new CounterState(0), CounterReducer);
var settingsSlice = Factory.CreateSlice("settings", new SettingsState(false), SettingsReducer);
var (initialRoot, rootReducer) = Factory.CombineReducers(counterSlice, settingsSlice);
```

Middleware example (persistence hook sketch):

```csharp
Middleware persist = new PersistMiddleware(new MySnapshotStore());
using var store = Factory.CreateStore(initialRoot, rootReducer, persist);
```

Subscribe to state changes (replay immediate):

```csharp
using var sub = store.StateChanged.Subscribe(state => Console.WriteLine("state changed"));
```

# Identity & Reducer Rules (critical)

- Reducers must return the same instance when nothing changed. AtomicState uses reference equality to decide when to
  publish a new root snapshot. Returning an equal-but-new instance causes an unnecessary publication.

- `CombineReducers` preserves root identity when every slice returns the same instance.

# Integration patterns & testing

- Keep binary/large native resources out of state; store ids/metadata and manage heavy resources elsewhere.
- Use middleware for side-effects (persistence, logging, network calls). Surface side-effect results as actions.
- For tests, create a seeded store via `CreateSeededStore` or `InMemorySnapshotStore`, dispatch recorded sequences, and
  assert final `store.State` and emitted actions.

# When Not to Use

- High-frequency inner-loop native state (per-token model internals) — keep those in native/host-managed contexts.
- Storing large binary blobs; prefer external snapshot storage via `ISnapshotStore`.

# References (source locations)

- `src/AtomicState/Store/Factory.cs` — factory helpers and overloads
- `src/AtomicState/Store/IStore.cs` — runtime store surface
- `src/AtomicState/Actions/IAction.cs` — action contracts
- `src/AtomicState/State` — slice and combine helpers
- `src/AtomicState/Persistence` — snapshot and persistence helpers
- `src/AtomicState/Middleware` — bundled middleware like logging and time-travel

# Maintenance

Keep this SKILL updated when public API names, overloads, or identity semantics change so agents can rely on it for
integration and example generation.
