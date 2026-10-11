namespace SnapshotStore
{
    /// <summary>
    /// Implements typed store runtime behavior for serialized dispatch, immutable state publication,
    /// and change notifications behind the public IStore contract.
    /// </summary>
    internal sealed class Store<TAction> : IStore<TAction> where TAction : class, IAction
    {
        // Disposal is tracked so dispatch after disposal fails deterministically.
        private volatile bool _isDisposed;

        // Single lock is sufficient because writes are serialized and state snapshots are immutable.
        private readonly object _lock = new();

        // Reducer is provided at composition time and treated as immutable runtime dependency.
        private readonly RootReducer<TAction> _reducer;

        // The fully composed pipeline is built once because its middleware order does not change after creation.
        private readonly Action<TAction> _dispatch;

        // Holds the latest published immutable snapshot.
        private StateSnapshot _state;

        public Store(
            StateSnapshot initialState,
            RootReducer<TAction> reducer,
            IReadOnlyList<DispatchMiddleware<TAction>> middleware)
        {
            if (initialState is null)
                throw new ArgumentNullException(nameof(initialState));

            if (reducer is null)
                throw new ArgumentNullException(nameof(reducer));

            if (middleware is null)
                throw new ArgumentNullException(nameof(middleware));

            // Initial snapshot is trusted immutable input from StoreFactory.
            _state = initialState;
            _reducer = reducer;

            Action<TAction> dispatch = DispatchCore;
            for (var index = middleware.Count - 1; index >= 0; index--)
            {
                var currentMiddleware = middleware[index]
                    ?? throw new ArgumentException("Middleware cannot contain null entries.", nameof(middleware));
                var next = dispatch;

                // Reverse composition makes the first registered middleware the outermost stage.
                dispatch = action =>
                {
                    var nextCalls = 0;
                    currentMiddleware(action, forwardedAction =>
                    {
                        // Repeated forwarding would run downstream middleware or reducers more than once.
                        if (Interlocked.Exchange(ref nextCalls, 1) != 0)
                            throw new InvalidOperationException("Middleware must invoke next at most once per action.");

                        next(forwardedAction);
                    });
                };
            }

            _dispatch = dispatch;
        }

        public event Action<StateSnapshot> OnStateChanged = delegate { };

        // Readers should observe the latest published immutable snapshot without paying lock costs.
        public StateSnapshot State => Volatile.Read(ref _state);

        public void Dispatch(TAction action)
        {
            // Null actions are programmer errors; rejecting them simplifies reducer assumptions.
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            // Reject calls after disposal before middleware can produce side effects; DispatchCore repeats
            // the check under the lock to cover a concurrent dispose between this check and reducer entry.
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Store<TAction>));

            _dispatch(action);
        }

        private void DispatchCore(TAction action)
        {
            Action<StateSnapshot> handlers;
            StateSnapshot nextState;

            lock (_lock)
            {
                // A disposed store should not permit further mutation attempts.
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(Store<TAction>));

                var currentState = _state;
                nextState = _reducer(currentState, action);

                // Reference equality enables cheap "no-op" dispatch semantics.
                if (ReferenceEquals(nextState, currentState))
                    return;

                // We publish an immutable snapshot; write barrier ensures concurrent readers do not
                // observe stale state after dispatch completes.
                Volatile.Write(ref _state, nextState);
                handlers = OnStateChanged;
            }

            // Notify outside lock to avoid deadlocks and re-entrance lock inversion.
            handlers(nextState);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                // Disposal is idempotent to support defensive multi-call cleanup code.
                if (_isDisposed)
                    return;

                _isDisposed = true;
            }
        }
    }
}