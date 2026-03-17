namespace SC.AtomicState.Runtime
{
    /// <summary>
    /// Implements the store that owns serialized dispatch, state publication, and observable notifications.
    /// </summary>
    internal sealed class Store : IStore
    {
        /// <summary>
        /// Reuses the internal initialization action.
        /// </summary>
        private static readonly IAction s_initAction = Factory.CreateAction("@@INIT");

        /// <summary>
        /// Serializes reducer execution and disposal transitions.
        /// </summary>
        private readonly Lock _dispatchLock = new();

        /// <summary>
        /// Stores the composed dispatch pipeline.
        /// </summary>
        private readonly Dispatch _dispatch;

        /// <summary>
        /// Publishes state updates to observers.
        /// </summary>
        private readonly StateStream _stateStream;

        /// <summary>
        /// Publishes the latest committed state for lock-free readers.
        /// </summary>
        private IState _state;

        /// <summary>
        /// Indicates whether the store has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Stores the root reducer.
        /// </summary>
        private readonly Reducer _rootReducer;

        /// <summary>
        /// Initializes a new store.
        /// </summary>
        /// <param name="initialState">The initial root state to expose before the initialization pass runs.</param>
        /// <param name="rootReducer">The root reducer that computes committed state transitions.</param>
        /// <param name="middleware">Optional middleware applied around the serialized dispatch core.</param>
        internal Store(IState initialState, Reducer rootReducer, params AtomicMiddleware[] middleware)
        {
            _rootReducer = rootReducer;
            _stateStream = new StateStream(initialState);
            _state = initialState;

            if (middleware.Length > 0)
            {
                var current = (Dispatch)DispatchCore;
                for (var index = middleware.Length - 1; index >= 0; index--)
                {
                    current = middleware[index](this, current);
                }

                _dispatch = current;
            }
            else
            {
                _dispatch = DispatchCore;
            }

            RunInitPass();
        }

        /// <summary>
        /// Gets the observable stream of state changes.
        /// </summary>
        /// <remarks>
        /// The returned stream replays the current committed state to new subscribers and receives live updates after
        /// dispatch commits a replacement state instance.
        /// </remarks>
        public IObservable<IState> StateChanged => _stateStream;

        /// <summary>
        /// Gets the current root state.
        /// </summary>
        /// <remarks>
        /// Uses an acquire read so un-synchronized readers can observe the latest committed state published by
        /// <see cref="Dispatch(IAction)"/>.
        /// </remarks>
        public IState State => Volatile.Read(ref _state);

        /// <summary>
        /// Dispatches an action through the composed middleware pipeline.
        /// </summary>
        /// <param name="action">The action to dispatch.</param>
        /// <exception cref="ObjectDisposedException">Thrown when dispatch reaches the runtime after the store has been disposed.</exception>
        /// <remarks>
        /// Middleware executes around one serialized dispatch core. If the reducer returns a new state instance, that
        /// state is published before observers are notified so direct <see cref="State"/> reads and stream delivery
        /// remain aligned.
        /// </remarks>
        public void Dispatch(IAction action) => _dispatch(action);

        /// <summary>
        /// Disposes the store and completes the state stream.
        /// </summary>
        /// <remarks>
        /// Disposal is idempotent. The disposed transition is serialized with dispatch so no later reducer execution can
        /// commit additional state after disposal wins.
        /// </remarks>
        public void Dispose()
        {
            lock (_dispatchLock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
            }

            _stateStream.Dispose();
        }

        /// <summary>
        /// Runs the active root reducer and publishes changed state.
        /// </summary>
        private void DispatchCore(IAction action)
        {
            IState? stateToPublish = null;

            lock (_dispatchLock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

                var currentState = _state;
                var nextState = _rootReducer(currentState, action);
                if (!ReferenceEquals(nextState, currentState))
                {
                    // Publish with release semantics before notifying observers so lock-free readers can observe the
                    // same committed state that the stream is about to emit.
                    Volatile.Write(ref _state, nextState);
                    stateToPublish = nextState;
                }
            }

            // Publish outside the reducer lock so observers cannot block future dispatches.
            if (stateToPublish is not null)
            {
                _stateStream.OnNext(stateToPublish);
            }
        }

        /// <summary>
        /// Dispatches the internal initialization action.
        /// </summary>
        /// <remarks>
        /// Initialization is part of successful store creation, so reducer or middleware failures are propagated to
        /// the caller instead of leaving a partially initialized store instance in circulation.
        /// </remarks>
        private void RunInitPass() => _dispatch(s_initAction);
    }
}