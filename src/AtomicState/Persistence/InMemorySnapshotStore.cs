using System.Collections.Concurrent;

namespace SC.AtomicState.Persistence
{
    /// <summary>
    /// A fast, thread-safe in-memory snapshot store useful for tests and examples.
    /// </summary>
    public sealed class InMemorySnapshotStore : ISnapshotStore
    {
        private readonly ConcurrentDictionary<string, IState> _store = new();

        public Task SaveAsync(string key, IState snapshot, CancellationToken ct = default)
        {
            _store[key] = snapshot;
            return Task.CompletedTask;
        }

        public Task<IState?> LoadAsync(string key, CancellationToken ct = default) =>
            Task.FromResult(_store.TryGetValue(key, out var s) ? s : null);
    }
}
