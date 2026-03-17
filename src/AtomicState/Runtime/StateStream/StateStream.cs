namespace SC.AtomicState.Runtime
{
    /// <summary>
    /// Coordinates replay, live publication, and terminal delivery for one store stream.
    /// </summary>
    internal sealed partial class StateStream(IState initialState) : IObservable<IState>, IDisposable
    {
        /// <summary>
        /// Tracks registrations that should receive future live publications.
        /// </summary>
        private readonly List<Registration> _observers = [];

        /// <summary>
        /// Protects stream state so subscription, publication, and termination observe one stream-level order.
        /// </summary>
        private readonly Lock _lock = new();

        /// <summary>
        /// Prevents new subscriptions after disposal and suppresses repeated disposal work.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Distinguishes a graceful terminal state from an active stream.
        /// </summary>
        private bool _isCompleted;

        /// <summary>
        /// Captures the terminal fault so future subscribers observe the same outcome.
        /// </summary>
        private Exception? _error;

        /// <summary>
        /// Retains the latest committed state for immediate replay to new subscribers.
        /// </summary>
        private IState? _lastState = initialState;

        /// <summary>
        /// Registers an observer and schedules its replay and terminal sequence in stream order.
        /// </summary>
        public IDisposable Subscribe(IObserver<IState> observer)
        {
            var registration = default(Registration);
            var shouldDrain = false;

            lock (_lock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(StateStream));

                registration = GetOrAddRegistration(observer);

                // Replay and termination are queued while the stream state is stable so subscribe cannot interleave
                // with a concurrent publish and reverse the observer's first visible ordering.
                if (_lastState is not null)
                {
                    shouldDrain |= registration.Enqueue(Notification.OnNext(_lastState));
                }

                if (_error is not null)
                {
                    shouldDrain |= registration.Enqueue(Notification.OnError(_error));
                }
                else if (_isCompleted)
                {
                    shouldDrain |= registration.Enqueue(Notification.OnCompleted());
                }
            }

            // Delivery stays outside the stream lock so user callbacks cannot stall other stream bookkeeping.
            registration.Drain(shouldDrain);

            return new Subscription(this, registration);
        }

        /// <summary>
        /// Publishes a new state snapshot to the observers that were active at commit time.
        /// </summary>
        public void OnNext(IState value)
        {
            Registration[] snapshot;
            lock (_lock)
            {
                if (_isDisposed || _isCompleted || _error is not null)
                {
                    return;
                }

                _lastState = value;
                snapshot = [.. _observers];
            }

            // The snapshot decouples callback-side subscription churn from the current publication pass.
            foreach (var observer in snapshot)
            {
                observer.Publish(Notification.OnNext(value));
            }
        }

        /// <summary>
        /// Faults the stream and guarantees that no later live publication will be accepted.
        /// </summary>
        public void OnError(Exception exception)
        {
            Registration[] snapshot;
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
                observer.Publish(Notification.OnError(exception));
            }
        }

        /// <summary>
        /// Completes the stream and detaches the live observer set.
        /// </summary>
        public void OnCompleted()
        {
            Registration[] snapshot;
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
                observer.Publish(Notification.OnCompleted());
            }
        }

        /// <summary>
        /// Transitions the stream to its disposed terminal state and completes remaining observers.
        /// </summary>
        public void Dispose()
        {
            Registration[] snapshot;
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
                observer.Publish(Notification.OnCompleted());
            }
        }

        /// <summary>
        /// Removes a registration from future live delivery without disturbing any queued callbacks it already owns.
        /// </summary>
        private void Unsubscribe(Registration registration)
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _ = _observers.Remove(registration);
            }
        }

        /// <summary>
        /// Reuses an existing live registration so one observer cannot receive duplicate live delivery paths.
        /// </summary>
        private Registration GetOrAddRegistration(IObserver<IState> observer)
        {
            foreach (var registration in _observers)
            {
                if (ReferenceEquals(registration.Observer, observer))
                {
                    return registration;
                }
            }

            var created = new Registration(observer);
            if (!_isCompleted && _error is null)
            {
                // Terminated streams still need deterministic replay/terminal delivery for future subscribers, but
                // those one-off registrations must not rejoin the live observer set.
                _observers.Add(created);
            }

            return created;
        }

        /// <summary>
        /// Shields the stream from observer completion failures.
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
        /// Shields the stream from observer fault-handling failures.
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
        /// Shields the stream from observer next-value failures.
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
    }
}