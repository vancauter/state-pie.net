# AtomicState Design v0.6.0 — Implementation Status

Date: 2026-03-26

Summary: Tracks implementation progress against the v0.6.0 design document (`docs/atomicstate-design-v0.6.0.md`).

Status:

- [x] Read design document — 2026-03-26
- [ ] Audit codebase for target files and public surface
- [x] Add `TypedActionId<T>` and `Factory.CreateAction<T>` — 2026-03-26
- [x] Add unit tests for typed-action helpers — 2026-03-26
- [ ] Implement `ISnapshotStore`, `InMemorySnapshotStore`, `ThrowingSnapshotStore`, `PersistMiddleware`
- [ ] Add `StoreSnapshotHelper` and `CreateSeededStore`
- [ ] Add `MiddlewareHelpers` and `DispatchAsync`/`Enqueue` helpers
- [ ] Add `IStoreMetrics` and `NoOpStoreMetrics` and wire into `Factory.CreateStore`
- [ ] Implement `TimetravelMiddleware` skeleton and controller
- [ ] Add example projects and docs (`reducer-guidelines`, `async-middleware`, `persistence`)
- [ ] Run build and tests (full verification)

Notes:

- The `TypedActionId<T>` helper and matching factory overload were implemented and unit-tested. Tests passed.
- Next step: perform a focused audit of the codebase to locate insertion points for persistence, helpers, and telemetry.

Next actions (short):

1. Audit the repository for `Factory`, `IDispatchContext`, `IAction`, and existing middleware locations.
2. Implement `ISnapshotStore` and test doubles under `src/AtomicState/Persistence/`.
