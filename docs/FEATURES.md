# AtomicRx — Suggested Feature Roadmap

This document lists suggested features to improve ergonomics, extensibility, and testability. Use this as a checklist; I
will update the tracked TODO list as I implement items.

- [ ] Middleware / Enhancers
- [x] Middleware / Enhancers
  - Add a middleware pipeline to intercept/augment `Dispatch` (e.g., logging, async middleware). Implemented: `Factory.CreateStore` accepts `Middleware[]` and `Store` composes pipeline.
  - [ ] Time-travel middleware (parked)
    - Record dispatched actions and root states to allow stepping back/forward, replay, and time-travel debugging.
    - Parked: will flesh out API shape and storage/replay semantics together with middleware work.

- [ ] Async action helpers (thunks)
- [x] Async action helpers (thunks)
  - Provide a small convention or middleware to handle function- or Task-based actions for async flows. Implemented: `ThunkMiddleware` supports sync `Action` and async `Func` thunks.

- [ ] Replace reducer / hot-swap
- [x] Replace reducer / hot-swap
  - `IStore.HotSwapReducer(Reducer newRoot)` implemented on `Store` (atomic swap + init pass).

- [ ] Init action / reducer initialization semantics
  - Document or provide an init action pattern so reducers can compute derived initial state.

- [ ] Observable error/completion semantics
  - Clarify or extend `StateChanged` observable with documented `OnError`/`OnCompleted` behavior or alternatives.

- [ ] Typed action creators / ergonomics
  - Provide helper patterns for typed action creators and constants to reduce boilerplate.

- [ ] Snapshot / deserialize / migrations
  - Add helpers to deserialize state, validate versions, and provide migration hooks.

- [ ] Immutability helpers / deep-equality
  - Small helpers and guidance for preserving reference-equality and performing deep comparisons when needed.

- [ ] Testing helpers & examples
  - Utilities and examples that simplify asserting store transitions, mocking reducers, and integration tests.

Notes

- Priority can be set per-item; I can implement these incrementally and check them off in the TODO list.
- Want me to start with `Middleware / Enhancers` and scaffold a minimal implementation and tests?
