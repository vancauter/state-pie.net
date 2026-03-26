# Persistence Adapter

- Use `ISnapshotStore` to persist snapshots: `SaveAsync(string key, IState snapshot)` and `LoadAsync(string key)`.
- Use `PersistMiddleware.Create(ISnapshotStore, string key, SnapshotOptions?)` to attach persistence behavior.
- Test doubles: `InMemorySnapshotStore` for unit tests and `ThrowingSnapshotStore` to simulate failures.
- Helpers: `StoreSnapshotHelper.LoadSnapshotAsync`, `StoreSnapshotHelper.SeedStoreAsync`,
  `CreateSeededStore.CreateAsync`.
- Best practice: persist snapshots after successful dispatches and surface follow-up success/failure actions.
