using System;

namespace SC.AtomicState.Persistence
{
    /// <summary>
    /// A test double that optionally throws on save and/or load operations to simulate failures.
    /// </summary>
    public sealed class ThrowingSnapshotStore : ISnapshotStore
    {
        private readonly bool _throwOnSave;
        private readonly bool _throwOnLoad;
        private readonly Exception _exception;

        public ThrowingSnapshotStore(bool throwOnSave = true, bool throwOnLoad = true, Exception? exception = null)
        {
            _throwOnSave = throwOnSave;
            _throwOnLoad = throwOnLoad;
            _exception = exception ?? new InvalidOperationException("Simulated snapshot store failure.");
        }

        public Task SaveAsync(string key, IState snapshot, CancellationToken ct = default)
        {
            if (_throwOnSave) throw _exception;
            return Task.CompletedTask;
        }

        public Task<IState?> LoadAsync(string key, CancellationToken ct = default)
        {
            if (_throwOnLoad) throw _exception;
            return Task.FromResult<IState?>(null);
        }
    }
}

