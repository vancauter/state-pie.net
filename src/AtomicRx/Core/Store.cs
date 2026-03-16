namespace AtomicRx.Core
{
    /// <summary>
    /// Implements the internal store runtime.
    /// </summary>
    internal sealed class Store : IStore
    {
        /// <summary>
        /// Indicates whether the store has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Stores the active root reducer.
        /// </summary>
        private Reducer _rootReducer;

        /// <summary>
        /// Publishes state changes to subscribers.
        /// </summary>
        private readonly StateSubject _stateSubject;

        /// <summary>
        /// Gets the observable stream of state changes.
        /// </summary>
        public IObservable<IState> StateChanged => _stateSubject;

        /// <summary>
        /// Gets the current root state.
        /// </summary>
        public IState State { get; private set; }

        /// <summary>
        /// Stores the composed dispatch pipeline.
        /// </summary>
        private readonly Dispatch _dispatch;

        /// <summary>
        /// Initializes a new store instance.
        /// </summary>
        internal Store(IState initialState, Reducer rootReducer, params Middleware[] middleware)
        {
            _rootReducer = rootReducer;
            _stateSubject = new StateSubject(initialState);
            State = initialState;

            void baseDispatch(IAction action)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

                var reducer = _rootReducer;
                var newState = reducer(State, action);

                if (!ReferenceEquals(newState, State))
                {
                    State = newState;
                    _stateSubject.OnNext(State);
                }
            }

            if (middleware != null && middleware.Length > 0)
            {
                var current = (Dispatch)baseDispatch;
                for (var i = middleware.Length - 1; i >= 0; i--)
                {
                    var mw = middleware[i];
                    current = mw(this, current);
                }
                _dispatch = current;
            }
            else
            {
                _dispatch = baseDispatch;
            }

            RunInitPass();
        }

        /// <summary>
        /// Disposes the store and completes the observable state stream.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _stateSubject.OnCompleted();
            _stateSubject.Dispose();
        }

        /// <summary>
        /// Dispatches an action through the middleware pipeline.
        /// </summary>
        public void Dispatch(IAction action) => _dispatch(action);

        /// <summary>
        /// Replaces the current root reducer and reruns the initialization pass.
        /// </summary>
        /// <param name="newRoot">New root reducer.</param>
        public void HotSwapReducer(Reducer newRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

            _ = Interlocked.Exchange(ref _rootReducer, newRoot);

            RunInitPass();
        }

        /// <summary>
        /// Dispatches the internal initialization action.
        /// </summary>
        private void RunInitPass()
        {
            try
            {
                var init = Factory.CreateAction("@@INIT");
                _dispatch(init);
            }
            catch
            {
                // Initialization is best-effort so startup does not fail on reducer bootstrap logic.
            }
        }
    }
}
