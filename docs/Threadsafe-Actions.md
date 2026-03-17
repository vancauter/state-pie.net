# AtomicState Thread-Safety Action Plan

## Objective

Close the known concurrency gaps in AtomicState in a safe implementation order, starting with correctness bugs in the
observable pipeline and then tightening publication semantics, middleware ordering guarantees, tests, and documentation.

## Resolved Contract Decisions

- `IStore.State` must provide strong cross-thread visibility for unsynchronized readers.
- `StateChanged` must provide strict linearized ordering across subscribe, replay, live updates, completion, and
  disposal races.
- Observer callbacks are serialized per store stream, but they run on the calling thread with no built-in thread
  affinity.
- Reentrant dispatch from middleware is supported.
- Custom middleware may schedule work outside the store's serialized dispatch timeline, but that scheduled work is
  outside the core ordering guarantee.
- When nested dispatch occurs, ordering is defined by the total serialized order of actual dispatch execution.

## Implementation Order

### Step 1: Write the contract down

- [ ] Document the resolved guarantees for concurrent `Dispatch`, strong `State` reads, `StateChanged` subscription, and
      serialized observer callbacks.
- [ ] Document that callbacks have no built-in thread affinity and may run on the dispatching or terminating thread.
- [ ] Explicitly state caller responsibilities, especially immutable state snapshots and pure reducers.

Why this comes first: the fixes below only make sense once the intended runtime contract is explicit.

### Step 2: Fix `StateStream` subscribe and terminal race conditions

- [ ] Rework `StateStream.Subscribe` so initial replay cannot race with `OnNext`, `OnCompleted`, `OnError`, or `Dispose`
      and produce out-of-order notifications.
- [ ] Ensure a subscriber cannot observe `OnCompleted` or `OnError` followed by a replayed `OnNext`.
- [ ] Ensure a subscriber cannot receive a newer live value before the initial replay of the last known state.
- [ ] Preserve serialized callback delivery while keeping observer invocation outside internal bookkeeping locks.

Why this is next: this is the highest-risk correctness bug because it can violate the observable contract even when
callers use the API correctly.

### Step 3: Add concurrency regression tests for `StateStream`

- [ ] Add tests that race `Subscribe` against `OnNext`.
- [ ] Add tests that race `Subscribe` against `OnCompleted` and `OnError`.
- [ ] Add tests that race `Subscribe` against `Dispose`.
- [ ] Verify notification order and terminal behavior, not just eventual outcomes.
- [ ] Verify callbacks remain serialized for a single store stream.

Why this follows immediately: `StateStream` is subtle enough that the fix should be locked in with repeatable
race-focused tests before moving on.

### Step 4: Implement strong `IStore.State` publication

- [ ] Implement strong publication for `State` so unsynchronized cross-thread readers observe the latest committed
      state.
- [ ] Choose the publication mechanism and keep the read path simple and defensible.
- [ ] Verify the `State` getter remains aligned with the store's committed dispatch order.

Why this is separate: `State` is a public API contract, not just an internal implementation detail.

### Step 5: Add `IStore.State` visibility tests

- [ ] Add tests that verify direct `State` reads stay current under concurrent dispatch.
- [ ] Add tests that confirm the value exposed through `State` matches the value most recently committed by the store.
- [ ] Add tests that cover reading `State` while observers are also receiving updates.

Why this matters: strong visibility is easy to claim and easy to get subtly wrong without dedicated tests.

### Step 6: Document and verify middleware ordering semantics

- [ ] Document that middleware may reenter `Dispatch` and that nested actions participate in one total serialized
      dispatch order.
- [ ] Document that custom middleware may schedule work outside the store's serialized dispatch timeline.
- [ ] Clarify that later dispatches from scheduled background work are still individually serialized, but are not part
      of the original dispatch transaction.

Why this is later: middleware behavior builds on the core store semantics, so it should be finalized after the base
runtime contract is stable.

### Step 7: Add middleware ordering regression tests

- [ ] Add tests that verify nested dispatch produces the documented serialized ordering.
- [ ] Add tests that verify middleware-triggered reentrant dispatch does not violate callback serialization.
- [ ] Add tests that show scheduled background work is outside the original dispatch transaction while its later
      dispatches still serialize correctly.

Why this is last: these guarantees only become meaningful once the underlying store and stream semantics are fixed.

## Exit Criteria

- `StateStream` no longer produces out-of-order replay or terminal notifications under concurrent use.
- Observer callbacks are serialized and documented as running on the calling thread with no built-in affinity.
- The `State` property has an explicit, implemented, and tested cross-thread visibility contract.
- Middleware reentrancy and nested dispatch ordering are documented and covered by tests.
- Concurrency guarantees are covered by automated tests.
