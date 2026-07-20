namespace SC.AtomicState.Persistence
{
    /// <summary>
    /// Provides an abstraction for saving and loading store snapshots.
    /// </summary>
    public interface ISnapshotStore
    {
        /// <summary>
        /// Persist the provided snapshot under the specified key.
        /// </summary>
        Task SaveAsync(string key, IState snapshot, CancellationToken ct = default);

        /// <summary>
        /// Load a previously saved snapshot identified by <paramref name="key"/>.
        /// Returns <see langword="null"/> when no snapshot exists for the key.
        /// </summary>
        Task<IState?> LoadAsync(string key, CancellationToken ct = default);
    }
}
