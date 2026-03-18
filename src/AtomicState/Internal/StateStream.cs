using Microsoft.Extensions.Logging;

namespace SC.AtomicState.Internal
{
    /// <summary>
    /// Coordinates replay, live publication, and terminal delivery for one store stream.
    /// </summary>
    internal sealed class StateStream(IState initialState, ILogger? logger = null) : IObservable<IState>, IDisposable
    {
        private readonly ILogger? _logger = logger;
        private readonly List<Registration> _observers = [];

        private readonly Lock _lock = new();

        private bool _isDisposed;

        private bool _isCompleted;

        private Exception? _error;

        private IState? _lastState = initialState;

        // LoggerMessage delegates for high-performance structured logging
        private static readonly Action<ILogger, string, Exception?> s_observerOnNextThrown =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1001, "ObserverOnNextThrew"), "StateStream observer {ObserverType} threw during OnNext");

        private static readonly Action<ILogger, string, Exception?> s_observerOnErrorThrown =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1002, "ObserverOnErrorThrew"), "StateStream observer {ObserverType} threw during OnError");

        private static readonly Action<ILogger, string, Exception?> s_observerOnCompletedThrown =
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(1003, "ObserverOnCompletedThrew"), "StateStream observer {ObserverType} threw during OnCompleted");

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

                registration = new Registration(observer, _logger);
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

            // Snapshot the observer set so callback-side subscription churn cannot perturb this publication pass.
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

        private static void TryNotifyCompleted(IObserver<IState> observer, ILogger? logger)
        {
            try
            {
                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                // Observer callbacks are isolated so one subscriber cannot interrupt terminal delivery.
                try
                {
                    if (logger is not null)
                    {
                        s_observerOnCompletedThrown(logger, observer?.GetType().FullName ?? "<null>", ex);
                    }
                }
                catch { }
            }
        }

        private static void TryNotifyError(IObserver<IState> observer, Exception exception, ILogger? logger)
        {
            try
            {
                observer.OnError(exception);
            }
            catch (Exception ex)
            {
                // Observer callbacks are isolated so one subscriber cannot interrupt terminal delivery.
                try
                {
                    if (logger is not null)
                    {
                        s_observerOnErrorThrown(logger, observer?.GetType().FullName ?? "<null>", ex);
                    }
                }
                catch { }
            }
        }

        private static void TryNotifyNext(IObserver<IState> observer, IState value, ILogger? logger)
        {
            try
            {
                observer.OnNext(value);
            }
            catch (Exception ex)
            {
                // Observer callbacks are isolated so one subscriber cannot interrupt stream publication.
                try
                {
                    if (logger is not null)
                    {
                        s_observerOnNextThrown(logger, observer?.GetType().FullName ?? "<null>", ex);
                    }
                }
                catch { }
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
        private sealed class Registration(IObserver<IState> observer, ILogger? logger)
        {
            public IObserver<IState> Observer { get; } = observer;

            private readonly ILogger? _logger = logger;

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
                        TryNotifyNext(Observer, notification.State!, _logger);
                        continue;
                    }

                    if (notification.Kind == NotificationKind.Error)
                    {
                        TryNotifyError(Observer, notification.Error!, _logger);
                        continue;
                    }

                    TryNotifyCompleted(Observer, _logger);
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