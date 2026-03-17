namespace SC.AtomicState.Runtime
{
    internal sealed partial class StateStream
    {
        /// <summary>
        /// Owns one observer's callback queue so all notifications for that observer stay serialized.
        /// </summary>
        private sealed class Registration(IObserver<IState> observer)
        {
            /// <summary>
            /// Exposes the observer identity used to deduplicate live registrations.
            /// </summary>
            public IObserver<IState> Observer { get; } = observer;

            /// <summary>
            /// Protects queue ownership so exactly one thread drains this observer at a time.
            /// </summary>
            private readonly Lock _gate = new();

            /// <summary>
            /// Buffers notifications that arrived while another thread was already delivering callbacks.
            /// </summary>
            private readonly Queue<Notification> _pending = [];

            /// <summary>
            /// Acts as the observer-local drain token.
            /// </summary>
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
                            // The token is released only after the queue is observed empty so the next publisher can
                            // safely take exclusive ownership of delivery.
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
    }
}