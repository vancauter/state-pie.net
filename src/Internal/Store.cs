namespace StatePie
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

        // Registrations are accessed only while holding _lock so baseline and candidate selections stay coherent.
        private readonly List<IStateSelectorSubscription> _subscriptions = new();

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

        // Readers should observe the latest published immutable snapshot without paying lock costs.
        public StateSnapshot State => Volatile.Read(ref _state);

        public IDisposable Subscribe(Action<StateSnapshot> listener)
        {
            if (listener is null)
                throw new ArgumentNullException(nameof(listener));

            // Root and selected observers share the same subscription lifecycle and dispatch path.
            return Subscribe(static state => state, listener);
        }

        public IDisposable Subscribe<TValue>(
            Func<StateSnapshot, TValue> selector,
            Action<TValue> listener,
            IEqualityComparer<TValue>? comparer = null)
        {
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));

            if (listener is null)
                throw new ArgumentNullException(nameof(listener));

            lock (_lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(Store<TAction>));

                // Capture the baseline and register atomically so no committed transition can fall between them.
                var subscription = new StateSelectorSubscription<TValue>(
                    selector,
                    listener,
                    comparer ?? EqualityComparer<TValue>.Default,
                    _state,
                    Unsubscribe);
                _subscriptions.Add(subscription);
                return subscription;
            }
        }

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
            StateSnapshot nextState;
            List<Action>? subscriptionNotifications = null;

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

                // Prepare every selection before mutating cached values or publishing state. If user selector code
                // throws, neither the state nor any subscription baseline is partially advanced.
                var subscriptionUpdates = new List<IStateSelectorUpdate>(_subscriptions.Count);
                foreach (var subscription in _subscriptions)
                    subscriptionUpdates.Add(subscription.Prepare(nextState));

                foreach (var update in subscriptionUpdates)
                {
                    var notification = update.Commit();
                    if (notification is not null)
                    {
                        subscriptionNotifications ??= new List<Action>();
                        subscriptionNotifications.Add(notification);
                    }
                }

                // We publish an immutable snapshot; write barrier ensures concurrent readers do not
                // observe stale state after dispatch completes.
                Volatile.Write(ref _state, nextState);
            }

            // Notify outside the lock so listeners can read or dispatch without blocking state commits.
            if (subscriptionNotifications is null)
                return;

            foreach (var notification in subscriptionNotifications)
                notification();
        }

        private void Unsubscribe(IStateSelectorSubscription subscription)
        {
            lock (_lock)
            {
                subscription.Deactivate();
                _subscriptions.Remove(subscription);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                // Disposal is idempotent to support defensive multi-call cleanup code.
                if (_isDisposed)
                    return;

                _isDisposed = true;
                foreach (var subscription in _subscriptions)
                    subscription.Deactivate();

                _subscriptions.Clear();
            }
        }
    }
}