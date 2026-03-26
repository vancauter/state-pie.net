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
- [ ] Implement `TimetravelMiddleware` skeleton and controller
- [ ] Add example projects and docs (`reducer-guidelines`, `async-middleware`, `persistence`)
- [ ] Run build and tests (full verification)

Notes:

The `TypedActionId<T>` helper and matching factory overload were implemented and unit-tested. Tests passed. Next step:
implement `PersistMiddleware` and add tests.

Next actions (short):

1. Implement `PersistMiddleware` and unit tests (in `src/AtomicState/Persistence/` and `tests/AtomicState`).
2. Add `StoreSnapshotHelper` and `CreateSeededStore`.
3. Add `MiddlewareHelpers` and async dispatch helpers.
4. Add `IStoreMetrics` and telemetry wiring.
