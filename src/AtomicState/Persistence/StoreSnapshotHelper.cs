namespace SC.AtomicState.Persistence
{
    /// <summary>
    /// Helper methods for working with store snapshots.
    /// </summary>
    public static class StoreSnapshotHelper
    {
        /// <summary>
        /// Loads a snapshot from <paramref name="store"/> using the provided <paramref name="key"/> and,
        /// if present, replaces the target store state by creating a seeded store via <see cref="Factory.CreateStore"/>.
        /// </summary>
        /// <remarks>
        /// This helper is intentionally small and synchronous from the caller's perspective; it returns the loaded
        /// snapshot or <see langword="null"/> if none was found. The caller can then choose how to apply the
        /// snapshot (for example, by creating a seeded store in tests or examples).
        /// </remarks>
        public static async Task<IState?> LoadSnapshotAsync(ISnapshotStore store, string key, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(store, nameof(store));
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key must be a non-empty string.", nameof(key));

            return await store.LoadAsync(key, ct).ConfigureAwait(false);
        }

        /// <summary>
        /// Applies <paramref name="snapshot"/> to <paramref name="store"/> without going through the normal
        /// dispatch pipeline. This helper attempts to seed the run-time store instance when the concrete implementation
        /// is the library's internal <c>Store</c>. When the store implementation cannot be seeded, a
        /// <see cref="NotSupportedException"/> is thrown.
        /// </summary>
        public static Task SeedStoreAsync(IStore store, IState snapshot)
        {
            ArgumentNullException.ThrowIfNull(store, nameof(store));
            ArgumentNullException.ThrowIfNull(snapshot, nameof(snapshot));

            // Try to detect the internal Store type by full name so this helper remains forward-compatible
            // if consumers use other IStore implementations.
            var storeType = store.GetType();
            if (storeType.FullName == "SC.AtomicState.Internal.Store")
            {
                // Set the private _state field and publish the snapshot to the internal StateStream so observers
                // see the seeded value. We intentionally avoid calling public Dispatch to prevent reducer runs.
                var stateField = storeType.GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var streamField = storeType.GetField("_stateStream", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (stateField is null || streamField is null)
                    throw new NotSupportedException("Store implementation does not expose expected backing fields.");

                stateField.SetValue(store, snapshot);

                var stream = streamField.GetValue(store);
                var onNext = stream?.GetType().GetMethod("OnNext", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                _ = (onNext?.Invoke(stream, [snapshot]));

                return Task.CompletedTask;
            }

            throw new NotSupportedException("Seeding the provided store instance is not supported for this store implementation.");
        }

        /// <summary>
        /// Captures the current committed root state from <paramref name="store"/>.
        /// </summary>
        public static Task<IState> CaptureSnapshotAsync(IStore store)
        {
            ArgumentNullException.ThrowIfNull(store, nameof(store));
            return Task.FromResult(store.State);
        }
    }
}
