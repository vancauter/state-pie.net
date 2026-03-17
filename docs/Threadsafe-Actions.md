# AtomicState Thread-Safety Status

## Objective

Track only the remaining thread-safety gaps in AtomicState. Completed work should be removed from this document as it
lands so the file stays focused on unfinished concurrency work.

## Resolved Contract Decisions

- `IStore.State` is intended to provide strong cross-thread visibility for unsynchronized readers.
- `StateChanged` must provide strict linearized ordering across subscribe, replay, live updates, completion, and
  disposal races.
- Observer callbacks are serialized per store stream, but they run on the calling thread with no built-in thread
  affinity.
- Reentrant dispatch from middleware is supported.
- Custom middleware may schedule work outside the store's serialized dispatch timeline, but that scheduled work is
  outside the core ordering guarantee.
- When nested dispatch occurs, ordering is defined by the total serialized order of actual dispatch execution.

## Remaining Gaps

### Strong `IStore.State` Publication

- The contract is documented, but the implementation is not yet defensible for unsynchronized cross-thread readers.
- `StoreRuntime.State` is currently a normal auto-property updated under the dispatch lock. That preserves reducer-side
  serialization, but it does not by itself establish a strong publication guarantee for lock-free readers.
- This still needs an explicit publication mechanism with a simple read path.

### Race-Focused Regression Coverage

- There are still no dedicated tests that repeatedly race `Subscribe` against `OnNext`.
- There are still no dedicated tests that repeatedly race `Subscribe` against `OnCompleted`, `OnError`, or `Dispose`.
- Current stream tests validate deterministic behavior in normal sequencing, but they do not stress the concurrency
  edges that motivated this work.

### `IStore.State` Visibility Tests

- There are no tests yet that verify direct `State` reads stay current under concurrent dispatch.
- There are no tests yet proving the value returned by `State` matches the most recently committed store state while
  observers are also receiving updates.

### Middleware Reentrancy Regression Coverage

- There are no dedicated tests yet for nested dispatch ordering through middleware.
- There are no tests yet proving middleware-triggered reentrant dispatch preserves callback serialization.
- There are no tests yet showing that background-scheduled middleware work falls outside the original dispatch
  transaction while its later dispatches still serialize correctly.

## Exit Criteria

- `StateStream` ordering and terminal behavior remain stable under race-focused concurrency tests.
- The `State` property has an explicit, implemented, and tested cross-thread visibility mechanism.
- Middleware reentrancy and nested dispatch ordering are covered by targeted regression tests.
- The published documentation matches the implementation and test suite.
