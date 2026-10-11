namespace SnapshotStore
{
    /// <summary>
    /// Represents a typed immutable store where dispatch accepts only one action family and publishes the latest state snapshot.
    /// </summary>
    public interface IStore<TAction> : IDisposable where TAction : class, IAction
    {
        StateSnapshot State { get; }

        /// <summary>
        /// Subscribes to every committed root snapshot change without replaying the current snapshot.
        /// </summary>
        /// <param name="listener">The callback invoked synchronously after each changed snapshot is committed.</param>
        /// <returns>A subscription that stops receiving future snapshots when disposed.</returns>
        IDisposable Subscribe(Action<StateSnapshot> listener);

        /// <summary>
        /// Subscribes to selected state changes without replaying the current value; callbacks run synchronously after commit and outside the store lock.
        /// </summary>
        /// <typeparam name="TValue">The selected value type.</typeparam>
        /// <param name="selector">A pure, fast function that reads a value from a state snapshot.</param>
        /// <param name="listener">The callback invoked when the selected value changes.</param>
        /// <param name="comparer">An optional equality comparer; the default comparer is used when omitted.</param>
        /// <returns>A subscription that stops receiving future selected-value changes when disposed.</returns>
        /// <remarks>
        /// The selector and comparer run during dispatch before the new snapshot is committed, so exceptions leave the current state unchanged. Listener exceptions propagate after commit and do not roll back state.
        /// </remarks>
        IDisposable Subscribe<TValue>(
            Func<StateSnapshot, TValue> selector,
            Action<TValue> listener,
            IEqualityComparer<TValue>? comparer = null);

        void Dispatch(TAction action);
    }
}