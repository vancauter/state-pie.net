# AtomicState Thread-Safety Status

## Objective

Track only the remaining thread-safety gaps in AtomicState. Completed work should be removed from this document as it
lands so the file stays focused on unfinished concurrency work.

## Resolved Contract Decisions

- `IStore.State` is intended to provide strong cross-thread visibility for un-synchronized readers.
- `StateChanged` must provide strict linearized ordering across subscribe, replay, live updates, completion, and
  disposal races.
- Observer callbacks are serialized per store stream, but they run on the calling thread with no built-in thread
  affinity.
- Reentrant dispatch from middleware is supported.
- Custom middleware may schedule work outside the store's serialized dispatch timeline, but that scheduled work is
  outside the core ordering guarantee.
- When nested dispatch occurs, ordering is defined by the total serialized order of actual dispatch execution.

## Remaining Gaps

### Race-Focused Regression Coverage

- There are still no dedicated tests that repeatedly race `Subscribe` against `OnNext`.
- There are still no dedicated tests that repeatedly race `Subscribe` against `OnCompleted`, `OnError`, or `Dispose`.
- Current stream tests validate deterministic behavior in normal sequencing, but they do not stress the concurrency
  edges that motivated this work.

### Middleware Re-entrance Regression Coverage

- There are no dedicated tests yet for nested dispatch ordering through middleware.
- There are no tests yet proving middleware-triggered reentrant dispatch preserves callback serialization.
- There are no tests yet showing that background-scheduled middleware work falls outside the original dispatch
  transaction while its later dispatches still serialize correctly.

## Exit Criteria

- `StateStream` ordering and terminal behavior remain stable under race-focused concurrency tests.
- Middleware re-entrance and nested dispatch ordering are covered by targeted regression tests.
- The published documentation matches the implementation and test suite.
