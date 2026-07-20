namespace SC.AtomicState.Internal
{
    /// <summary>
    /// Coordinates replay, live publication, and terminal delivery for one store stream.
    /// </summary>
    internal sealed class StateStream(IState initialState) : IObservable<IState>, IDisposable
    {
        private readonly List<Registration> _observers = [];

        private readonly Lock _lock = new();

        private bool _isDisposed;

        private bool _isCompleted;

        private Exception? _error;

        private IState? _lastState = initialState;

        /// <summary>
        /// Registers an observer and schedules its replay and terminal sequence in stream order.
        /// </summary>
        public IDisposable Subscribe(IObserver<IState> observer)
        {
            ArgumentNullException.ThrowIfNull(observer);

            var registration = default(Registration);
            var shouldDrain = false;

            lock (_lock)
            {
                ObjectDisposedException.ThrowIf(_isDisposed, nameof(StateStream));

                registration = new Registration(observer);
                if (!_isCompleted && _error is null)
                {
                    _observers.Add(registration);
                }

                // Queue replay and any terminal signal while the stream state is stable so subscribe cannot interleave
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

            // Deliver outside the stream lock so user callbacks cannot stall other stream bookkeeping.
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

            // Make a copy of the observer list before notifying them so any subscriptions/un-subscriptions
            // that happen during callbacks won't affect this notification round
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

        private static void TryNotifyCompleted(IObserver<IState> observer)
        {
            try
            {
                observer.OnCompleted();
            }
            catch
            {
                // Swallow observer exceptions.
            }
        }

        private static void TryNotifyError(IObserver<IState> observer, Exception exception)
        {
            try
            {
                observer.OnError(exception);
            }
            catch
            {
                // Swallow observer exceptions.
            }
        }

        private static void TryNotifyNext(IObserver<IState> observer, IState value)
        {
            try
            {
                observer.OnNext(value);
            }
            catch
            {
                // Swallow observer exceptions.
            }
        }

        /// <summary>
        /// Captures one queued callback so replay, live values, and termination share the same delivery path.
        /// </summary>
        private readonly record struct Notification(NotificationKind Kind, IState? State, Exception? Error)
        {
            public static Notification OnCompleted() => new(NotificationKind.Completed, null, null);

            public static Notification OnError(Exception error) => new(NotificationKind.Error, null, error);

            public static Notification OnNext(IState state) => new(NotificationKind.Next, state, null);
        }

        private enum NotificationKind
        {
            Next,
            Error,
            Completed
        }

        /// <summary>
        /// Owns one subscription's callback queue so replay, live delivery, and terminal signals share one ordered path.
        /// </summary>
        private sealed class Registration(IObserver<IState> observer)
        {
            public IObserver<IState> Observer { get; } = observer;

            private readonly Lock _gate = new();

            private readonly Queue<Notification> _pending = [];

            private bool _isDraining;

            /// <summary>
            /// Appends work and lets the winning thread deliver it through the shared drain loop.
            /// </summary>
            public void Publish(Notification notification) => Drain(Enqueue(notification));

            /// <summary>
            /// Enqueues work and reports whether this caller became the observer's active drainer.
            /// </summary>
            public bool Enqueue(Notification notification)
            {
                lock (_gate)
                {
                    _pending.Enqueue(notification);
                    if (_isDraining)
                    {
                        // Appending behind the active drainer preserves callback order without recursive delivery.
                        return false;
                    }

                    _isDraining = true;
                    return true;
                }
            }

            /// <summary>
            /// Delivers queued callbacks until another thread must take over.
            /// </summary>
            public void Drain(bool shouldDrain)
            {
                if (!shouldDrain)
                {
                    return;
                }

                while (true)
                {
                    Notification notification;

                    lock (_gate)
                    {
                        if (_pending.Count == 0)
                        {
                            // Release the token only after the queue is observed empty so the next publisher can take
                            // exclusive ownership of delivery safely.
                            _isDraining = false;
                            return;
                        }

                        notification = _pending.Dequeue();
                    }

                    if (notification.Kind == NotificationKind.Next)
                    {
                        TryNotifyNext(Observer, notification.State!);
                        continue;
                    }

                    if (notification.Kind == NotificationKind.Error)
                    {
                        TryNotifyError(Observer, notification.Error!);
                        continue;
                    }

                    TryNotifyCompleted(Observer);
                }
            }
        }

        /// <summary>
        /// Keeps unsubscribe tied to the specific registration created for the observer.
        /// </summary>
        private sealed class Subscription(StateStream owner, Registration registration) : IDisposable
        {
            private readonly StateStream _owner = owner;
            private readonly Registration _registration = registration;

            /// <summary>
            /// Stops future live delivery for this registration.
            /// </summary>
            public void Dispose() => _owner.Unsubscribe(_registration);
        }
    }
}
