namespace SCLibs.StateContainer.Core
{
    // Lightweight, internal subject optimized for broadcasting IState instances.
    // Behaviors:
    // - Thread-safe: subscription list guarded by a private lock.
    // - Immediately emits the last known state to newly subscribed observers.
    // - Swallows exceptions from observer callbacks to keep dispatching to other observers.
    internal sealed class StateSubject(IState initialState) : IObservable<IState>, IDisposable
    {
        private readonly List<IObserver<IState>> _observers = [];
        private readonly object _lock = new();
        private bool _isDisposed;
        private IState? _lastState = initialState;

        // Subscribe registers the observer and immediately emits the last state (if present).
        public IDisposable Subscribe(IObserver<IState> observer)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(StateSubject));

            IState? initialStateToEmit;
            lock (_lock)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);

                initialStateToEmit = _lastState;
            }

            if (initialStateToEmit is not null)
                try { observer.OnNext(initialStateToEmit); } catch { }

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

            foreach (var o in snapshot)
            {
                try { o.OnNext(value); } catch { }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
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
