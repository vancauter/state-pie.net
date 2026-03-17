namespace AtomicRx
{
    /// <summary>
    /// Represents a state store that owns the current root state and processes dispatched actions.
    /// </summary>
    /// <remarks>
    /// Implementations expose the latest state through <see cref="State"/> and publish it through the <see cref="StateChanged"/> state stream.
    /// </remarks>
    public interface IStore : IDisposable
    {
        /// <summary>
        /// Gets the current root state snapshot.
        /// </summary>
        IState State { get; }

        /// <summary>
        /// Gets an observable sequence that emits the root state whenever it changes.
        /// </summary>
        /// <remarks>
        /// Subscribers receive the current state immediately upon subscription. The state stream completes when the store is disposed.
        /// </remarks>
        IObservable<IState> StateChanged { get; }

        /// <summary>
        /// Dispatches an action to the store.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the store has already been disposed.</exception>
        /// <remarks>
        /// Dispatch is serialized by the store implementation so reducer execution and state publication remain ordered.
        /// </remarks>
        void Dispatch(IAction action);
    }
}