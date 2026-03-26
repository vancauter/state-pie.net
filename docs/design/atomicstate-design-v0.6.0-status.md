# AtomicState Design v0.6.0 — Implementation Status

Date: 2026-03-26

Summary: Tracks implementation progress against the v0.6.0 design document (`docs/atomicstate-design-v0.6.0.md`).

Status:

- [x] Read design document — 2026-03-26
- [x] Audit codebase for target files and public surface — 2026-03-26
- [x] Add `TypedActionId<T>` and `Factory.CreateAction<T>` — 2026-03-26
- [x] Add unit tests for typed-action helpers — 2026-03-26
- [x] Implement `ISnapshotStore`, `InMemorySnapshotStore`, `ThrowingSnapshotStore` — 2026-03-26
- [ ] Implement `PersistMiddleware` and tests
- [ ] Add `StoreSnapshotHelper` and `CreateSeededStore`
- [ ] Add `MiddlewareHelpers` and `DispatchAsync`/`Enqueue` helpers
- [x] Implement `PersistMiddleware` and tests — 2026-03-26
- [x] Add `StoreSnapshotHelper` and `CreateSeededStore` — 2026-03-26
- [ ] Add `MiddlewareHelpers` and `DispatchAsync`/`Enqueue` helpers
- [ ] Add `IStoreMetrics` and `NoOpStoreMetrics` and wire into `Factory.CreateStore`
- [x] Add `MiddlewareHelpers` and `DispatchAsync`/`Enqueue` helpers — 2026-03-26
- [x] Add `IStoreMetrics` and `NoOpStoreMetrics` and wire into `Factory.CreateStore` — 2026-03-26
- [x] Implement `TimetravelMiddleware` skeleton and controller — 2026-03-26
- [ ] Add example projects and docs (`reducer-guidelines`, `async-middleware`, `persistence`)
- [ ] Run build and tests (full verification)

Completed examples actions:

- [x] Created top-level `examples/` snippets (basic, persistence, middleware, timetravel, metrics) — 2026-03-26
- [x] Removed ad-hoc `dev/Examples` project and moved examples to `examples/` — 2026-03-26
- [x] Added slice usage examples (flat + nested guidance) and reordered examples with basic → advanced — 2026-03-26

Notes:

The `TypedActionId<T>` helper and matching factory overload were implemented and unit-tested. Tests passed. Next step:
implement `PersistMiddleware` and add tests.

Next actions (short):

1. Implement `MiddlewareHelpers` and `DispatchContext.SafeDispatchAsync` helpers, plus unit tests demonstrating safe
   background dispatch patterns.
2. Expand `TimeTravelMiddleware` to capture snapshots/actions, implement undo/redo/replay behavior, and add the full
   suite of timetravel tests.
3. Expand `TimetravelMiddleware` tests and implement undo/redo/replay behavior.
4. Add example projects and `docs/reducer-guidelines.md`, `docs/async-middleware.md`, and `docs/persistence.md`.
5. Finalize status file with test results and mark v0.6.0 implementation complete when all acceptance criteria are met.

Notes (recent):

- Minimal `TimeTravelMiddleware` and `TimeTravelController` implemented under `src/AtomicState/Middleware/` (namespace
  `SC.AtomicState.MiddlewareEx`) and expanded tests added at `tests/AtomicState/TimetravelTests.cs` covering
  record/undo/redo.
- Persist middleware and tests added; background and synchronous save modes covered. `StoreSnapshotHelper` gains
  `SeedStoreAsync` and `CaptureSnapshotAsync` helpers. -- Test run: non-performance test suite passed locally (72/72) on
  2026-03-26.

- Examples: created `examples/` snippets demonstrating basic usage, middleware scheduling, persistence, timetravel
  usage, and a metrics example. These are top-level `.cs` snippets for documentation and quick copy/paste.
