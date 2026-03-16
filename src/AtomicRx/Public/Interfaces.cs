namespace AtomicRx
{
    /// <summary>
    /// Represents a non-generic action dispatched to a store.
    /// Actions are simple messages identified by a <see cref="Type"/> string.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Gets the string identifier for the action.
        /// </summary>
        string Type { get; }
    }

    /// <summary>
    /// Represents an action carrying a strongly-typed payload.
    /// </summary>
    /// <typeparam name="TPayload">The payload type for the action.</typeparam>
    public interface IAction<TPayload> : IAction
    {
        /// <summary>
        /// Gets the payload carried by the action.
        /// </summary>
        TPayload Payload { get; }
    }

    /// <summary>
    /// Marker interface for types that may be used as application state.
    /// Implementations should be immutable or treated as immutable by reducers.
    /// </summary>
    public interface IState { }

    /// <summary>
    /// Represents a named slice of the overall application state.
    /// A slice provides its current <see cref="State"/> and a <see cref="Reducer"/> to handle actions for that slice.
    /// </summary>
    public interface ISlice
    {
        /// <summary>
        /// Gets the unique name of this slice within a combined root state.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the current state object for this slice.
        /// </summary>
        IState State { get; }

        /// <summary>
        /// Gets the reducer that accepts the slice state and an action and returns the next slice state.
        /// </summary>
        Reducer Reducer { get; }
    }

    /// <summary>
    /// Represents the root store which holds the application state and handles dispatching actions.
    /// </summary>
    public interface IStore : IDisposable
    {
        /// <summary>
        /// Gets the current root state of the application.
        /// </summary>
        IState State { get; }

        /// <summary>
        /// Gets an observable stream that emits the root state each time it changes.
        /// </summary>
        /// <remarks>
        /// New subscribers immediately receive the current state. The observable completes when the store is disposed.
        /// Observer callback exceptions are suppressed so one subscriber cannot disrupt the rest of the notification pipeline.
        /// </remarks>
        IObservable<IState> StateChanged { get; }

        /// <summary>
        /// Dispatches an action to the store, causing reducers to compute the next state.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the store has already been disposed.</exception>
        void Dispatch(IAction action);

        /// <summary>
        /// Replaces the root reducer used by the store.
        /// </summary>
        /// <param name="newRoot">New root reducer to use for subsequent dispatches.</param>
        /// <exception cref="ObjectDisposedException">Thrown when the store has already been disposed.</exception>
        /// <remarks>
        /// The swap is atomic. After replacing the reducer, the store dispatches an internal initialization action so the new reducer can recompute any derived state.
        /// </remarks>
        void HotSwapReducer(Reducer newRoot);
    }
}
