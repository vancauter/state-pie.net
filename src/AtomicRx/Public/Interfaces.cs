namespace AtomicRx
{
    /// <summary>
    /// Represents a non-generic action dispatched to a store.
    /// Actions are simple, serializable messages identified by a <see cref="Type"/> string.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// A string identifier for the action type. Consumers should use meaningful, unique strings.
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
        /// The payload carried by the action.
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
        /// The unique name of this slice within a combined root state.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The current state object for this slice.
        /// </summary>
        IState State { get; }

        /// <summary>
        /// The reducer function that accepts the slice state and an action and returns the next slice state.
        /// </summary>
        Reducer Reducer { get; }
    }

    /// <summary>
    /// Represents the root store which holds the application state and handles dispatching actions.
    /// </summary>
    public interface IStore : IDisposable
    {
        /// <summary>
        /// The current root state of the application.
        /// </summary>
        IState State { get; }

        /// <summary>
        /// An observable stream that emits the root state each time it changes.
        ///
        /// Semantics:
        /// - The observable immediately emits the current state to new subscribers.
        /// - The observable will call <c>OnCompleted</c> when the store is disposed.
        /// - The observable does not emit errors for reducer exceptions; reducer exceptions
        ///   are expected to be handled by callers or middleware. Observer callback
        ///   exceptions are swallowed to avoid disrupting other subscribers.
        /// </summary>
        IObservable<IState> StateChanged { get; }

        /// <summary>
        /// Dispatches an action to the store, causing reducers to compute the next state.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        void Dispatch(IAction action);

        /// <summary>
        /// Replace the root reducer used by the store. The replacement is atomic and the
        /// store will run an internal init pass after swapping so the new reducer can
        /// compute any derived initial state.
        /// </summary>
        /// <param name="newRoot">New root reducer to use for subsequent dispatches.</param>
        void HotSwapReducer(Reducer newRoot);
    }
}
