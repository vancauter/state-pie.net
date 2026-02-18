# AtomicRx

AtomicRx is a small, focused reactive state-management library for .NET. It provides a lightweight Store and subscription primitives designed for explicit, reference-based change detection and safe multi-threaded dispatch.

## What it is

- A Redux-like store that expects immutable state snapshots.
- A thread-safe SimpleSubject implementation that exposes state as an immediately-replaying observable.
- Utilities for composing state from independent "slices" with atomic updates.

## When to use it

- UI applications that require push-based updates and benefit from an instant replay of the latest state.
- Scenarios where state is treated as immutable snapshots and cheap reference checks are preferred.
- Multi-threaded environments where dispatch and subscription must be safe and predictable.
- Unity Games: Works well with Unity's typical update patterns and was designed with Unity usage in mind.

## Key features

- Reactive replay: new subscribers receive the last known state immediately.
- Reference-strict change detection: notifications are issued only when a reducer returns a new instance (uses ReferenceEquals).
- Atomic composition: `CombineReducers` composes independent slice reducers into a single, atomic update.
- Thread-safe dispatch + subscription: locking ensures safe use from multiple threads.
- Explicit slicing: the global state is represented as a dictionary of slices for predictable structure.

## Design principles

- No Change, No Noise: avoid updates unless the state instance changes.
- Immutability by convention: the API assumes you will treat state as immutable snapshots.
- Simplicity and performance: keep the implementation small and predictable.
