namespace SC.AtomicState.Persistence
{
    /// <summary>
    /// Helpers to create a store seeded from a snapshot store when available.
    /// </summary>
    public static class CreateSeededStore
    {
        /// <summary>
        /// Loads a snapshot from <paramref name="snapshotStore"/> by <paramref name="key"/> and creates an
        /// <see cref="IStore"/> using the loaded snapshot as the initial state if present; otherwise uses
        /// <paramref name="initialState"/>.
        /// </summary>
        public static async Task<IStore> CreateAsync(ISnapshotStore snapshotStore, string key, IState initialState, Reducer rootReducer, CancellationToken ct = default, params Middleware[] middleware)
        {
            ArgumentNullException.ThrowIfNull(snapshotStore, nameof(snapshotStore));
            ArgumentNullException.ThrowIfNull(initialState, nameof(initialState));
            ArgumentNullException.ThrowIfNull(rootReducer, nameof(rootReducer));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key must be a non-empty string.", nameof(key));

            var loaded = await snapshotStore.LoadAsync(key, ct).ConfigureAwait(false);
            var seed = loaded ?? initialState;
            return Factory.CreateStore(seed, rootReducer, middleware);
        }
    }
}
