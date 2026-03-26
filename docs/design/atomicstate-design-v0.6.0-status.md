# AtomicState Design v0.6.0 — Implementation Status

Date: 2026-03-26

Summary: Tracks implementation progress against the v0.6.0 design document (`docs/atomicstate-design-v0.6.0.md`).

Status:

- [x] Read design document — 2026-03-26
- [x] Audit codebase for target files and public surface — 2026-03-26
- [x] Add `TypedActionId<T>` and `Factory.CreateAction<T>` — 2026-03-26
- [x] Add unit tests for typed-action helpers — 2026-03-26
- [x] Implement `ISnapshotStore`, `InMemorySnapshotStore`, `ThrowingSnapshotStore` — 2026-03-26
- [x] Implement `PersistMiddleware` and tests — 2026-03-26
- [x] Add `StoreSnapshotHelper` and `CreateSeededStore` — 2026-03-26
- [x] Add `MiddlewareHelpers` and `DispatchAsync`/`Enqueue` helpers — 2026-03-26
- [x] Add `IStoreMetrics` and `NoOpStoreMetrics` and wire into `Factory.CreateStore` — 2026-03-26
- [x] Implement `TimeTravelMiddleware` skeleton and controller — 2026-03-26
- [x] Expand `TimeTravelMiddleware` to enforce `MaxHistory`, suppress recording during time-travel, and implement
      `ReplayAsync` — 2026-03-26
- [x] Add example projects and docs (`reducer-guidelines`, `async-middleware`, `persistence`) — 2026-03-26 (moved to
      `docs/documentation`)
- [x] Run build and tests (non-performance) — 2026-03-26 (76/76)
- [x] Update `README.md` and `.github/skills/atomic-state/SKILL.md` to conform to the codebase — 2026-03-26

Completed examples actions:

- [x] Created top-level `examples/` snippets (basic, persistence, middleware, time-travel, metrics) — 2026-03-26
- [x] Removed ad-hoc `dev/Examples` project and moved examples to `examples/` — 2026-03-26
- [x] Added slice usage examples (flat + nested guidance) and reordered examples with basic → advanced — 2026-03-26

Notes:

The `TypedActionId<T>` helper and matching factory overload were implemented and unit-tested. Persist middleware,
snapshot helpers, `MiddlewareHelpers`, and `IStoreMetrics` were added. `TimeTravelMiddleware` and its controller were
expanded to enforce `MaxHistory`, suppress recording during time-travel operations, and provide a simple `ReplayAsync`
mechanism. All non-performance tests pass locally (76/76) as of 2026-03-26.

Final status:

- [x] v0.6.0 implementation complete — 2026-03-26 (all acceptance criteria met; tests: 76/76 non-performance)

Next actions (short):

- None left.

Notes (recent):

- `TimeTravelMiddleware` and `TimeTravelController` implemented and expanded under `src/AtomicState/Middleware/`
  (`SC.AtomicState.MiddlewareEx`) with tests at `tests/AtomicState/TimeTravelTests.cs` covering record/undo/redo.
- Persist middleware and tests added; background and synchronous save modes covered. `StoreSnapshotHelper` provides
  `SeedStoreAsync` and `CaptureSnapshotAsync` helpers.
- Examples: top-level `examples/` snippets created demonstrating basic usage, middleware scheduling, persistence,
  time-travel usage, and a metrics example.
