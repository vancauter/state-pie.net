namespace AtomicRx.Runtime
{
    /// <summary>
    /// Implements a replaying observable used by the store runtime.
    /// </summary>
    internal sealed class ReplayStateStream(IState initialState) : IObservable<IState>, IDisposable
    {
        /// <summary>
        /// Stores the subscribed observers.
        /// </summary>
        private readonly List<IObserver<IState>> _observers = [];

        /// <summary>
        /// Synchronizes observer and terminal state changes.
        /// </summary>
        private readonly Lock _lock = new();

        /// <summary>
        /// Indicates whether the stream has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Indicates whether completion has been signaled.
        /// </summary>
        private bool _isCompleted;

        /// <summary>
        /// Stores the terminal error, when one has been published.
        /// </summary>
        private Exception? _error;

        /// <summary>
        /// Stores the most recently published state.
        /// </summary>
        private IState? _lastState = initialState;

        /// <summary>
        /// Subscribes an observer and replays the latest known state.
        /// </summary>
        public IDisposable Subscribe(IObserver<IState> observer)
        {
            bool shouldReplayState;
            bool shouldComplete;
            Exception? error;
            IState? stateToReplay;

            lock (_lock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(ReplayStateStream));

                shouldComplete = _isCompleted;
                error = _error;
                stateToReplay = _lastState;
                shouldReplayState = stateToReplay is not null;

                if (!shouldComplete && error is null && !_observers.Contains(observer))
                {
                    _observers.Add(observer);
                }
            }

            if (shouldReplayState)
            {
                TryNotifyNext(observer, stateToReplay!);
            }

            if (error is not null)
            {
                TryNotifyError(observer, error);
            }
            else if (shouldComplete)
            {
                TryNotifyCompleted(observer);
            }

            return new Subscription(this, observer);
        }

        /// <summary>
        /// Publishes a state update to all current observers.
        /// </summary>
        public void OnNext(IState value)
        {
            IObserver<IState>[] snapshot;
            lock (_lock)
            {
                if (_isDisposed || _isCompleted || _error is not null)
                {
                    return;
                }

                _lastState = value;
                snapshot = [.. _observers];
            }

            foreach (var observer in snapshot)
            {
                TryNotifyNext(observer, value);
            }
        }

        /// <summary>
        /// Publishes a terminal error to all current observers.
        /// </summary>
        public void OnError(Exception exception)
        {
            IObserver<IState>[] snapshot;
            lock (_lock)
            {
                if (_isDisposed || _isCompleted || _error is not null)
                {
                    return;
                }

                _error = exception;
                _isCompleted = true;
                snapshot = [.. _observers];
                _observers.Clear();
            }

            foreach (var observer in snapshot)
            {
                TryNotifyError(observer, exception);
            }
        }

        /// <summary>
        /// Completes the observable sequence.
        /// </summary>
        public void OnCompleted()
        {
            IObserver<IState>[] snapshot;
            lock (_lock)
            {
                if (_isDisposed || _isCompleted || _error is not null)
                {
                    return;
                }

                _isCompleted = true;
                snapshot = [.. _observers];
                _observers.Clear();
            }

            foreach (var observer in snapshot)
            {
                TryNotifyCompleted(observer);
            }
        }

        /// <summary>
        /// Disposes the stream and completes any remaining observers.
        /// </summary>
        public void Dispose()
        {
            IObserver<IState>[] snapshot;
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;
                _isCompleted = true;
                snapshot = [.. _observers];
                _observers.Clear();
            }

            foreach (var observer in snapshot)
            {
                TryNotifyCompleted(observer);
            }
        }

        /// <summary>
        /// Removes an observer from the subscription list.
        /// </summary>
        private void Unsubscribe(IObserver<IState> observer)
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _ = _observers.Remove(observer);
            }
        }

        /// <summary>
        /// Attempts to notify an observer that the sequence has completed.
        /// </summary>
        private static void TryNotifyCompleted(IObserver<IState> observer)
        {
            try
            {
                observer.OnCompleted();
            }
            catch
            {
            }
        }

        /// <summary>
        /// Attempts to notify an observer about a terminal error.
        /// </summary>
        private static void TryNotifyError(IObserver<IState> observer, Exception exception)
        {
            try
            {
                observer.OnError(exception);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Attempts to notify an observer about a new state value.
        /// </summary>
        private static void TryNotifyNext(IObserver<IState> observer, IState value)
        {
            try
            {
                observer.OnNext(value);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Represents an observer subscription.
        /// </summary>
        private sealed class Subscription(ReplayStateStream owner, IObserver<IState> observer) : IDisposable
        {
            /// <summary>
            /// Stores the owning state stream.
            /// </summary>
            private readonly ReplayStateStream _owner = owner;

            /// <summary>
            /// Stores the subscribed observer.
            /// </summary>
            private readonly IObserver<IState> _observer = observer;

            /// <summary>
            /// Removes the observer from the owning stream.
            /// </summary>
            public void Dispose() => _owner.Unsubscribe(_observer);
        }
    }
}