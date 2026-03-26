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
    }
}
