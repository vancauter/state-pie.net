# AtomicRx Action Document

## Objective

Bring the shipped library, test coverage, and public documentation into alignment for AtomicRx's current scope as a
minimal reactive state container.

## Priority Actions

### P1: Stop hiding initialization failures

- [ ] Replace the blanket `catch` in store initialization with an explicit strategy.
- [ ] Decide whether initialization should fail fast, surface diagnostics, or expose an opt-in error hook.
- [ ] Add tests for reducer or middleware failures during the internal `@@INIT` dispatch.

Why this matters: initialization problems are currently suppressed, which can leave the store in an unexpected state
without any signal to the caller.

### P2: Back the thread-safety claim with tests

- [ ] Add concurrency-focused tests for simultaneous dispatch from multiple tasks.
- [ ] Verify ordered publication and final state correctness under contention.
- [ ] Document any guarantees that are intentionally out of scope, such as observer timing or middleware reentrancy.

Why this matters: the README promises serialized, thread-safe dispatch, but the current suite only exercises
single-threaded and delayed async scenarios.

### P3: Future enhancement

- [ ] Evaluate whether time-travel middleware belongs in the near-term roadmap after the stability and documentation
      gaps above are closed.

Why this matters: time-travel debugging is useful, but it should not outrank correctness, diagnostics, and test coverage
work.

## Exit Criteria

- Store initialization failures have an intentional and tested contract.
- Concurrency guarantees are validated by automated tests.
- Performance Test
