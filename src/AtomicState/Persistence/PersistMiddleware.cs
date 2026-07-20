namespace SC.AtomicState.Persistence
{
    /// <summary>
    /// Provides middleware that saves snapshots to an <see cref="ISnapshotStore"/> after dispatch.
    /// </summary>
    public static class PersistMiddleware
    {
        /// <summary>
        /// Creates middleware that saves the current root state to <paramref name="store"/> under <paramref name="key"/> after each dispatched action.
        /// </summary>
        /// <param name="store">Snapshot store to persist into.</param>
        /// <param name="key">Key used for saving snapshots.</param>
        /// <param name="getState">A delegate that returns the current root <see cref="IState"/>. The delegate may capture the store variable; it is invoked after the dispatch completes.</param>
        /// <param name="saveInBackground">When true the snapshot save is scheduled on the thread-pool. When false the save is performed synchronously on the dispatch caller thread.</param>
        public static Middleware Create(ISnapshotStore store, string key, Func<IState> getState, bool saveInBackground = true)
        {
            ArgumentNullException.ThrowIfNull(store);
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key must be a non-empty string.", nameof(key));
            ArgumentNullException.ThrowIfNull(getState);

            return (context, next) => action =>
            {
                _ = context; // intentionally referenced to satisfy unused-parameter analyzers
                next(action);

                if (saveInBackground)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var snapshot = getState();
                            await store.SaveAsync(key, snapshot).ConfigureAwait(false);
                        }
                        catch
                        {
                            // Swallow persistence errors to avoid impacting dispatch semantics.
                        }
                    });
                }
                else
                {
                    try
                    {
                        var snapshot = getState();
                        store.SaveAsync(key, snapshot).GetAwaiter().GetResult();
                    }
                    catch
                    {
                        // Swallow persistence errors to avoid impacting dispatch semantics.
                    }
                }
            };
        }
    }
}
