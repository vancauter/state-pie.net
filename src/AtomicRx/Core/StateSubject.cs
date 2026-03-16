namespace AtomicRx.Core
{
    /// <summary>
    /// Implements a stateful observable used by the store.
    /// </summary>
    internal sealed class StateSubject(IState initialState) : IObservable<IState>, IDisposable
    {
        /// <summary>
        /// Stores the active observers.
        /// </summary>
        private readonly List<IObserver<IState>> _observers = [];

        /// <summary>
        /// Guards observer and terminal-state mutations.
        /// </summary>
        private readonly object _lock = new();

        /// <summary>
        /// Indicates whether the subject has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Indicates whether the subject has completed.
        /// </summary>
        private bool _isCompleted;

        /// <summary>
        /// Stores the terminal error, when one has been signaled.
        /// </summary>
        private Exception? _error;

        /// <summary>
        /// Stores the most recently observed state.
        /// </summary>
        private IState? _lastState = initialState;

        /// <summary>
        /// Subscribes an observer and replays the latest state when available.
        /// </summary>
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
                if (err is not null)
                    try { observer.OnError(err); } catch { }
                else
                    try { observer.OnCompleted(); } catch { }
            }

            return new Subscription(() => { lock (_lock) { _ = _observers.Remove(observer); } });
        }

        /// <summary>
        /// Publishes a new state to all observers.
        /// </summary>
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

        /// <summary>
        /// Signals an error to all observers.
        /// </summary>
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

        /// <summary>
        /// Signals completion to all observers.
        /// </summary>
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

        /// <summary>
        /// Releases the subject and clears its observer list.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            OnCompleted();
            lock (_lock) { _observers.Clear(); }
        }

        /// <summary>
        /// Represents a disposable observer subscription.
        /// </summary>
        private sealed class Subscription(System.Action disposeAction) : IDisposable
        {
            /// <summary>
            /// Stores the unsubscription callback.
            /// </summary>
            private readonly System.Action _disposeAction = disposeAction;

            /// <summary>
            /// Removes the associated observer.
            /// </summary>
            public void Dispose() => _disposeAction();
        }
    }
}
