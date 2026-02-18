namespace SCLibs.StateContainer.Core
{
    // Internal store implementation. Responsible for holding the root state, applying the root reducer
    // and notifying subscribers when the state reference changes. Supports optional middleware that
    // can intercept or augment dispatched actions.
    internal sealed class Store : IStore
    {
        private bool _isDisposed;
        private Reducer _rootReducer;
        private readonly object _reducerLock = new();

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

            // Base dispatch: apply the root reducer and publish the new state if the reference changed.
            // Note: this method intentionally does not catch exceptions from reducers — callers or middleware
            // should handle errors as appropriate. Also, we check disposal at the start to avoid races
            // where a dispatch occurs while the store is being disposed.
            void baseDispatch(IAction action)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

                Reducer reducer;
                lock (_reducerLock) { reducer = _rootReducer; }

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

        /// <summary>
        /// Replace the current root reducer used by the store. Thread-safe.
        /// </summary>
        /// <param name="newReducer">New reducer to use for subsequent dispatches.</param>
        public void ReplaceReducer(Reducer newReducer)
        {
            if (newReducer == null) throw new ArgumentNullException(nameof(newReducer));
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

            lock (_reducerLock)
            {
                _rootReducer = newReducer;
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            // Dispose internal subject which clears observers; further Dispatch calls will throw.
            _stateSubject.Dispose();
        }

        // Dispatch calls the composed middleware pipeline (or the base dispatch when no middleware).
        // Middleware receives the store and a `next` Dispatch delegate to call the next stage in the pipeline.
        public void Dispatch(IAction action) => _dispatch(action);
    }
}