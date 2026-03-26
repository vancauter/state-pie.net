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
- [ ] Add `IStoreMetrics` and `NoOpStoreMetrics` and wire into `Factory.CreateStore`
- [x] Implement `TimetravelMiddleware` skeleton and controller — 2026-03-26
- [ ] Add example projects and docs (`reducer-guidelines`, `async-middleware`, `persistence`)
- [ ] Run build and tests (full verification)

Notes:

The `TypedActionId<T>` helper and matching factory overload were implemented and unit-tested. Tests passed. Next step:
implement `PersistMiddleware` and add tests.

Next actions (short):

1. Expand `TimetravelMiddleware` to capture snapshots/actions, implement undo/redo/replay behavior, and add the full
   suite of timetravel tests.
2. Add example projects and `docs/reducer-guidelines.md`, `docs/async-middleware.md`, and `docs/persistence.md`.
3. Finalize status file with test results and mark v0.6.0 implementation complete when all acceptance criteria are met.

Notes (recent):

- Minimal `TimetravelMiddleware` and `TimetravelController` implemented under `src/AtomicState/Middleware/` (namespace
  `SC.AtomicState.MiddlewareEx`) and a basic unit test added at `tests/AtomicState/TimetravelTests.cs`.
- Test run: non-performance test suite passed locally (68/68) on 2026-03-26.
