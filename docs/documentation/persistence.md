# Persistence & Snapshot Middleware

Summary

- Use `SC.AtomicState.Persistence.ISnapshotStore` to persist and restore root-state snapshots.
- `SC.AtomicState.Persistence.PersistMiddleware.Create(...)` saves snapshots after dispatch and can operate
  synchronously or in background.

Test helpers

- `SC.AtomicState.Persistence.InMemorySnapshotStore` — fast deterministic in-memory store for tests.
- `SC.AtomicState.Persistence.ThrowingSnapshotStore` — simulate failures.
- `SC.AtomicState.Persistence.StoreSnapshotHelper` — `SeedStoreAsync` and `CaptureSnapshotAsync` helpers for
  deterministic tests.

Usage

```csharp
var store = Factory.CreateStore(initialState, rootReducer, PersistMiddleware.Create(storeImpl, "key", () => store.State));
```
