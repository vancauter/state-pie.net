namespace AtomicRx.Core
{
    // Internal subject implementation that holds the current state and manages observer subscriptions.
    // Behaviors:
    // - Thread-safe: subscription list guarded by a private lock.
    // - Immediately emits the last known state to newly subscribed observers.
    // - Emits OnCompleted when the subject is completed/disposed.
    // - Swallows exceptions from observer callbacks to keep dispatching to other observers.
    internal sealed class StateSubject(IState initialState) : IObservable<IState>, IDisposable
    {
        private readonly List<IObserver<IState>> _observers = [];
        private readonly object _lock = new();
        private bool _isDisposed;
        private bool _isCompleted;
        private Exception? _error;
        private IState? _lastState = initialState;

        // Subscribe registers the observer and immediately emits the last state (if present).
        public IDisposable Subscribe(IObserver<IState> observer)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(StateSubject));

            bool completedOrErrored;
            Exception? err;
            IState? initialStateToEmit;

            lock (_lock)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);

                completedOrErrored = _isCompleted || _error is not null;
                err = _error;
                initialStateToEmit = _lastState;
            }

            if (initialStateToEmit is not null)
                try { observer.OnNext(initialStateToEmit); } catch { }

            if (completedOrErrored)
            {
                // If the subject has already completed or errored, immediately notify the new subscriber.
                if (err is not null)
                    try { observer.OnError(err); } catch { }
                else
                    try { observer.OnCompleted(); } catch { }
            }

            // Return an IDisposable that removes the observer when disposed.
            return new Subscription(() => { lock (_lock) { _ = _observers.Remove(observer); } });
        }

        // OnNext publishes a new state to all observers. It updates the last state and sends a snapshot
        // of observers to avoid holding the lock while calling external code.
        public void OnNext(IState value)
        {
            if (_isDisposed) return;

            IObserver<IState>[] snapshot;
            lock (_lock)
            {
                _lastState = value;
                snapshot = [.. _observers];
            }

            foreach (var o in snapshot) { try { o.OnNext(value); } catch { } }
        }

        // Signal an error to all observers and mark the subject as completed with error.
        public void OnError(Exception ex)
        {
            if (_isDisposed) return;

            IObserver<IState>[] snapshot;
            lock (_lock)
            {
                _error = ex;
                _isCompleted = true;
                snapshot = [.. _observers];
            }

            foreach (var o in snapshot) { try { o.OnError(ex); } catch { } }
        }

        // Signal completion to all observers. After completion no further OnNext calls will be delivered.
        public void OnCompleted()
        {
            if (_isDisposed) return;

            IObserver<IState>[] snapshot;
            lock (_lock)
            {
                _isCompleted = true;
                snapshot = [.. _observers];
            }

            foreach (var o in snapshot) { try { o.OnCompleted(); } catch { } }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            // Ensure observers are notified of completion before clearing.
            OnCompleted();
            lock (_lock) { _observers.Clear(); }
        }

        // Internal class for managing observer subscriptions.
        // It captures a dispose action that removes the observer from the subject.
        private sealed class Subscription(System.Action disposeAction) : IDisposable
        {
            private readonly System.Action _disposeAction = disposeAction;

            public void Dispose() => _disposeAction();
        }
    }
}
