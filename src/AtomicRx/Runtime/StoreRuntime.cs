namespace AtomicRx.Runtime
{
    /// <summary>
    /// Implements the internal store runtime.
    /// </summary>
    internal sealed class StoreRuntime : IStore
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
        private readonly ReplayStateStream _stateStream;

        /// <summary>
        /// Indicates whether the store has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Stores the active root reducer.
        /// </summary>
        private Reducer _rootReducer;

        /// <summary>
        /// Initializes a new store runtime.
        /// </summary>
        internal StoreRuntime(IState initialState, Reducer rootReducer, params Middleware[] middleware)
        {
            _rootReducer = rootReducer;
            _stateStream = new ReplayStateStream(initialState);
            State = initialState;

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
        public IObservable<IState> StateChanged => _stateStream;

        /// <summary>
        /// Gets the current root state.
        /// </summary>
        public IState State { get; private set; }

        /// <summary>
        /// Dispatches an action through the composed middleware pipeline.
        /// </summary>
        public void Dispatch(IAction action) => _dispatch(action);

        /// <summary>
        /// Disposes the store and completes the state stream.
        /// </summary>
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
        /// Replaces the active root reducer and reruns initialization.
        /// </summary>
        public void HotSwapReducer(Reducer newRoot)
        {
            lock (_dispatchLock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(StoreRuntime));
                _rootReducer = newRoot;
            }

            RunInitPass();
        }

        /// <summary>
        /// Runs the active root reducer and publishes changed state.
        /// </summary>
        private void DispatchCore(IAction action)
        {
            IState? stateToPublish = null;

            lock (_dispatchLock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(StoreRuntime));

                var nextState = _rootReducer(State, action);
                if (!ReferenceEquals(nextState, State))
                {
                    State = nextState;
                    stateToPublish = nextState;
                }
            }

            if (stateToPublish is not null)
            {
                _stateStream.OnNext(stateToPublish);
            }
        }

        /// <summary>
        /// Dispatches the internal initialization action.
        /// </summary>
        private void RunInitPass()
        {
            try
            {
                _dispatch(s_initAction);
            }
            catch
            {
            }
        }
    }
}