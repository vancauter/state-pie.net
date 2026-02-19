# AtomicRx — Suggested Feature Roadmap

- [ ] Middleware / Enhancers
  - [ ] Time-travel middleware (parked)
    - Record dispatched actions and root states to allow stepping back/forward, replay, and time-travel debugging.
  - Parked: will flesh out API shape and storage/replay semantics together with middleware work.

- [ ] Snapshot / deserialize / migrations (parked)
  - Add helpers to deserialize state, validate versions, and provide migration hooks.

- [ ] Testing helpers & examples
  - Utilities and examples that simplify asserting store transitions, mocking reducers, and integration tests.
