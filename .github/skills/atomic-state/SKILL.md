---
name: atomic-state
description:
    Reference and integration guide for the AtomicState reactive state container (actions, reducers, slices, stores,
    middleware).
---

# Purpose

Capture the design, contracts, and recommended usage patterns for AtomicState so Cortex contributors can consult this
note instead of re-reading the library sources.

This SKILL documents the important APIs, concurrency and identity rules, middleware and composition patterns, and
practical integration points for using AtomicState alongside Cortex runtime components (snapshot stores, host/agent
orchestration, dashboard/state registry).

# When to use

- When you need a small, deterministic, observable application-state container for dashboards, session registries, or
  orchestration state.
- When you prefer reducer-driven deterministic updates that are simple to unit test and replay.
- When you need serialized dispatch ordering and immediate replay of the latest state to new subscribers.

Do not use AtomicState for hot inner loops or native model/LLama context state (keep model weights and per-token loops
in native paths for performance).

# Key Concepts & Contracts

- IAction / IAction<TPayload>
    - Immutable-by-convention message objects dispatched to stores. Action `Type` strings are the primary discriminant.

- Reducer / Reducer<TState>
    - Pure function that computes the next state for a slice or root from the current state and an `IAction`.
    - Must return the same instance when nothing changed. AtomicState uses reference identity to detect changes and
      avoid unnecessary publication.

- SliceHandle<TState>
    - Typed handle returned by `Factory.CreateSlice(...)`. Use handles when composing slices and later reading slice
      state via `StateAccess`.

- IStore
    - The root store interface exposing `IState State` and `IObservable<IState> StateChanged`.
    - `Dispatch(IAction)` is serialized: nested dispatches (from middleware) participate in the same serialized order.
    - `StateChanged` is replaying (subscribers immediately receive the latest state) and serialized per store stream.

- Factory helpers
    - `Factory.CreateAction(...)`, `Factory.CreateSlice(...)`, `Factory.CombineReducers(...)`,
      `Factory.CreateStore(...)`.
    - `CombineReducers` returns a combined root state and a root reducer for the produced shape.

- Middleware
    - Composed around dispatch in registration order. Middleware receives an `IDispatchContext` allowing nested
      `Dispatch` calls.
    - Suitable for logging, side-effects, and integration hooks (e.g., persisting snapshots or invoking external
      services).

# Concurrency & Ordering Guarantees

- Dispatch execution is serialized by the `IStore` implementation. Reducers run under that serialization.
- `IStore.State` is intended to provide strong cross-thread visibility of the latest committed root snapshot.
- `StateChanged` preserves a strict linearized ordering across subscription replay, live updates, completion, and
  disposal.
- Observer callbacks are serialized per store stream and run on the calling thread; there is no implicit scheduler.

# Identity-Based Change Detection (Important)

- AtomicState relies on reducers returning the same instance for no-op actions. If a reducer allocates an equal-but-new
  object for no-op, the store will treat it as a change and publish it.
- Design reducers to return `state` when no meaningful change occurs.
- When composing slices, each slice must preserve the same instance for no-op; `Factory.CombineReducers(...)` preserves
  root-state identity when every slice returns the same instance.

# Typical Usage Patterns

1. Single-root state (simple app):

```csharp
var store = Factory.CreateStore(initialRootState, rootReducer);
store.Dispatch(Factory.CreateAction("MY/DO_THING"));
var current = store.State;
```

2. Slice composition (recommended for multi-part state):

```csharp
var counterSlice = Factory.CreateSlice("counter", new CounterState(0), CounterReducer);
var personSlice = Factory.CreateSlice("person", new PersonState("Guest",30), PersonReducer);
var (rootState, rootReducer) = Factory.CombineReducers(counterSlice, personSlice);
using var store = Factory.CreateStore(rootState, rootReducer);
```

3. Observing state:

```csharp
using var sub = store.StateChanged.Subscribe(new Observer()); // gets immediate current state
```

4. Middleware for side-effects (e.g., persist snapshot): create middleware that intercepts actions and calls
   `ISnapshotStore.SaveAsync` as a side-effect, dispatching follow-up success/failure actions.

# Practical Integration Points with Cortex

- Dashboard / UI state
    - Use AtomicState to hold sessions, available snapshots, host/engine health, and UI selection state. New UI
      subscribers get the current state immediately.

- Snapshot registry
    - Implement a `SnapshotSlice` that stores snapshot metadata (id, timestamp, model checksum, n_ctx, size) and actions
      `SNAPSHOT/SAVE`, `SNAPSHOT/DELETE`, `SNAPSHOT/RESTORE`.
    - Middleware persist side-effect: on `SNAPSHOT/SAVE`, middleware triggers `ISnapshotStore.SaveAsync` and dispatches
      `SNAPSHOT/SAVED` or `SNAPSHOT/FAILED`.

- Multi-agent orchestration
    - Model agents and their status can be represented as slices for deterministic orchestration tests and replay.
    - Actors or TPL Dataflow mailboxes can own executors and publish outcome events to AtomicState (separation of
      concerns: actors own runtime state; AtomicState owns global app/coordination state).

- Testing & deterministic replay
    - Capture action sequences in tests and replay them to reproduce scenarios: helpful for orchestration regression
      tests.

# Pitfalls & Best Practices

- Follow the identity rule strictly for reducers. Prefer `return state;` for no-op branches rather than returning new
  instances with equal values.
- Keep heavy native resources (LLama weights, contexts) out of the store; store only metadata and references/ids
  pointing to native resources managed elsewhere.
- Avoid using `StateChanged` callbacks for heavy synchronous work — dispatch asynchronous work from middleware or
  schedule it off-thread.
- Use middleware for side-effects and ensure side-effect failures are surfaced as actions so state remains the single
  source of truth for outcomes.

# Testing patterns

- Unit test reducers directly using inputs and expected state references/values.
- Integration tests: create a store with slices/middleware, dispatch an action sequence, assert final `store.State` and
  emitted actions/events.
- When testing middleware side-effects that call `ISnapshotStore`, use the `InMemorySnapshotStore` or a test double and
  assert that `SaveAsync` was invoked and follow-up actions were dispatched.

# When NOT to use AtomicState

- Do not use AtomicState to manage per-token generation state or LLama native contexts — those are performance-sensitive
  and should remain in native/adapter-managed objects (executors owned by hosts or actors).
- Avoid storing large binary blobs in the store; store metadata and let `ISnapshotStore` implementations persist binary
  payloads externally.

# References (source locations)

- Factory & Store implementation: src/AtomicState/Store/Factory.cs and src/AtomicState/Store/Store.cs
- Store interface: src/AtomicState/Store/IStore.cs
- Slice & reducer helpers: src/AtomicState/State/\*.cs
- Middleware: src/AtomicState/Middleware/\*
- Example usage and README: README.md in the AtomicState repo root

# Quick integration checklist for Cortex

- [ ] Add `AtomicState` project reference to `src/Cortex.Dashboard` or other control plane projects that require
      observable state.
- [ ] Model `Snapshot` metadata as a slice and write middleware that calls `ISnapshotStore` implementations already
      present in Cortex.
- [ ] Keep native model/context instances inside `IModelHost`/executors; publish only ids/metadata into the store.
- [ ] Add unit tests that replay action sequences to verify orchestrator behavior.

# Contact / maintenance notes

- This SKILL is a living summary. If `AtomicState` source changes significantly (API rename, identity semantics
  changes), update this SKILL to reflect those changes so Cortex contributors can rely on it for integration guidance.
