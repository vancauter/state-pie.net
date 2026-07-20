namespace SC.AtomicState
{
    /// <summary>
    /// Represents a state store that owns the current root state and processes dispatched actions.
    /// </summary>
    /// <remarks>
    /// Implementations expose the latest state through <see cref="State"/> and publish it through the <see cref="StateChanged"/> state stream.
    /// Dispatch execution is serialized by the store so reducer execution, committed state publication, and observable
    /// notifications follow one store-defined order.
    /// </remarks>
    public interface IStore : IDisposable
    {
        /// <summary>
        /// Gets the current root state snapshot.
        /// </summary>
        /// <remarks>
        /// The intended concurrency contract is that cross-thread readers that are not synchronized observe the latest
        /// committed root state.
        /// </remarks>
        IState State { get; }

        /// <summary>
        /// Gets an observable sequence that emits the root state whenever it changes.
        /// </summary>
        /// <remarks>
        /// Subscribers receive the current state immediately upon subscription. The intended concurrency contract is
        /// strict linearized ordering across subscription replay, live updates, completion, and disposal. Observer
        /// callbacks are serialized per store stream, run on the calling thread, and have no built-in thread affinity
        /// or scheduler. The state stream completes when the store is disposed.
        /// </remarks>
        IObservable<IState> StateChanged { get; }

        /// <summary>
        /// Dispatches an action to the store.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the store has already been disposed.</exception>
        /// <remarks>
        /// Dispatch is serialized by the store implementation so reducer execution and state publication remain ordered.
        /// Nested dispatch triggered by middleware participates in the same total serialized dispatch order.
        /// </remarks>
        void Dispatch(IAction action);
    }
}