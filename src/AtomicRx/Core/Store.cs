namespace AtomicRx.Core
{
    // Internal store implementation. Responsible for holding the root state, applying the root reducer
    // and notifying subscribers when the state reference changes. Supports optional middleware that
    // can intercept or augment dispatched actions.
    internal sealed class Store : IStore
    {
        private bool _isDisposed;
        private Reducer _rootReducer;

        // Internal subject that manages subscriptions and emits the last seen state immediately
        // to new subscribers. See StateSubject for thread-safety and snapshot semantics.
        private readonly StateSubject _stateSubject;

        // Observable stream of state changes. Exposes the internal subject but only as IObservable.
        public IObservable<IState> StateChanged => _stateSubject;

        // Current root state instance.
        // Reducers must return the same instance when no change occurs; the store uses reference-equality
        // to detect changes and avoid unnecessary notifications.
        public IState State { get; private set; }

        // The composed dispatch pipeline built from middleware; defaults to the base reducer-applier.
        // Middleware functions are invoked in the order they are provided to CreateStore.
        private readonly Dispatch _dispatch;

        internal Store(IState initialState, Reducer rootReducer, params Middleware[] middleware)
        {
            _rootReducer = rootReducer;
            _stateSubject = new StateSubject(initialState);
            State = initialState;

            // Run init pass so reducers can compute derived initial state.
            RunInitPass();

            // Base dispatch: apply the root reducer and publish the new state if the reference changed.
            // Note: this method intentionally does not catch exceptions from reducers — callers or middleware
            // should handle errors as appropriate. Also, we check disposal at the start to avoid races
            // where a dispatch occurs while the store is being disposed.
            void baseDispatch(IAction action)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

                // Capture current reducer for this dispatch to avoid races with HotSwapReducer
                var reducer = _rootReducer;
                var newState = reducer(State, action);

                if (!ReferenceEquals(newState, State))
                {
                    State = newState;
                    // Publish the updated state to observers.
                    _stateSubject.OnNext(State);
                }
            }

            // If middleware is provided, build the pipeline by composing them around baseDispatch.
            // Composition order: middleware provided in array order will be invoked in that same order
            // when dispatching (first -> second -> ... -> base). We build the pipeline by folding
            // the middleware from last to first so that the first middleware wraps the second, etc.
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
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            // Complete the subject so subscribers receive OnCompleted, then dispose.
            _stateSubject.OnCompleted();
            _stateSubject.Dispose();
        }

        // Dispatch calls the composed middleware pipeline (or the base dispatch when no middleware).
        // Middleware receives the store and a `next` Dispatch delegate to call the next stage in the pipeline.
        public void Dispatch(IAction action) => _dispatch(action);

        /// <summary>
        /// Atomically replace the root reducer used by the store. After swapping the reducer
        /// the store runs an internal init pass so the new reducer can compute derived state.
        /// </summary>
        /// <param name="newRoot">New root reducer.</param>
        /// <remarks>
        /// The reducer swap is atomic and thread-safe. Dispatches that occur during the swap will
        /// either see the old or new reducer, but will not observe a partially swapped state.
        /// The init pass is also run after the swap to allow the new reducer to compute any derived
        /// initial state immediately. This is important for scenarios like hot-reloading where the new reducer
        /// may need to adjust the state shape or compute derived values based on the existing state.
        /// Callers should be aware that the init pass dispatches a special "@@INIT" action,
        /// and reducers should handle this action appropriately
        /// (e.g., by returning the current state or computing derived state without side effects).
        /// </remarks>
        public void HotSwapReducer(Reducer newRoot)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

            // Atomically swap the reducer reference.
            _ = Interlocked.Exchange(ref _rootReducer, newRoot);

            // Run init pass so the new reducer can compute derived initial state.
            RunInitPass();

        }

        // Internal method to run the init pass by dispatching a special "@@INIT" action. This allows reducers
        // to compute derived initial state immediately after being installed or hot-swapped.
        private void RunInitPass()
        {
            try
            {
                var init = Factory.CreateAction("@@INIT");
                var reducer = _rootReducer;
                var newState = reducer(State, init);

                if (!ReferenceEquals(newState, State))
                {
                    State = newState;
                    _stateSubject.OnNext(State);
                }
            }
            catch
            {
                // Swallow init errors; reducers should avoid throwing during init.
            }
        }
    }
}