namespace SC.AtomicState.Tests.Shared
{
    /// <summary>
    /// Adapts delegates into an observer so tests can assert notifications without defining one-off observer types.
    /// </summary>
    internal sealed class CallbackObserver(Action<IState> onNext, Action<Exception>? onError = null, Action? onCompleted = null) : IObserver<IState>
    {
        /// <inheritdoc />
        public void OnCompleted() => onCompleted?.Invoke();

        /// <inheritdoc />
        public void OnError(Exception error) => onError?.Invoke(error);

        /// <inheritdoc />
        public void OnNext(IState value) => onNext(value);
    }

    /// <summary>
    /// Collects published states into an external list when the test already owns the destination buffer.
    /// </summary>
    internal sealed class ListObserver(List<IState> destination) : IObserver<IState>
    {
        /// <inheritdoc />
        public void OnCompleted()
        {
        }

        /// <inheritdoc />
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc />
        public void OnNext(IState value) => destination.Add(value);
    }

    /// <summary>
    /// Records the last notification outcomes so tests can inspect replay, completion, and error behavior after the fact.
    /// </summary>
    internal sealed class RecordingObserver : IObserver<IState>
    {
        public List<IState> States { get; } = [];

        public int CompletedCount { get; private set; }

        public Exception? Error { get; private set; }

        public void Clear()
        {
            // Reuse the same observer across test phases without carrying forward prior terminal state.
            States.Clear();
            CompletedCount = 0;
            Error = null;
        }

        /// <inheritdoc />
        public void OnCompleted() => CompletedCount++;

        /// <inheritdoc />
        public void OnError(Exception error) => Error = error;

        /// <inheritdoc />
        public void OnNext(IState value) => States.Add(value);
    }

    /// <summary>
    /// Forces observer callbacks to throw so tests can verify that the stream swallows subscriber failures.
    /// </summary>
    internal sealed class ThrowingObserver(bool throwOnNext = false, bool throwOnError = false, bool throwOnCompleted = false) : IObserver<IState>
    {
        /// <inheritdoc />
        public void OnCompleted()
        {
            if (throwOnCompleted)
            {
                throw new InvalidOperationException("completed");
            }
        }

        /// <inheritdoc />
        public void OnError(Exception error)
        {
            if (throwOnError)
            {
                throw new InvalidOperationException("error");
            }
        }

        /// <inheritdoc />
        public void OnNext(IState value)
        {
            if (throwOnNext)
            {
                throw new InvalidOperationException("next");
            }
        }
    }

    /// <summary>
    /// Records typed values behind a lock so race tests can assert notification sequences without introducing their own data races.
    /// </summary>
    internal sealed class ConcurrentRecordingObserver<TState, TValue>(Func<TState, TValue> selector) : IObserver<IState>
        where TState : class, IState
    {
        private readonly Lock _gate = new();
        private readonly List<TValue> _values = [];
        private int _completedCount;

        public int CompletedCount
        {
            get
            {
                lock (_gate)
                {
                    return _completedCount;
                }
            }
        }

        public Exception? Error
        {
            get
            {
                lock (_gate)
                {
                    // Guard reads with the same lock used for writes so tests never observe torn terminal state.
                    return field;
                }
            }

            private set;
        }

        public TValue[] GetValues()
        {
            lock (_gate)
            {
                return [.. _values];
            }
        }

        /// <inheritdoc />
        public void OnCompleted()
        {
            lock (_gate)
            {
                _completedCount++;
            }
        }

        /// <inheritdoc />
        public void OnError(Exception error)
        {
            lock (_gate)
            {
                Error = error;
            }
        }

        /// <inheritdoc />
        public void OnNext(IState value)
        {
            if (value is not TState typedValue)
            {
                throw new InvalidCastException($"Expected observer value of type '{typeof(TState).Name}', but got '{value.GetType().Name}'.");
            }

            lock (_gate)
            {
                _values.Add(selector(typedValue));
            }
        }
    }

    /// <summary>
    /// Satisfies observer subscriptions in tests where only subscription side effects matter.
    /// </summary>
    internal sealed class NoOpObserver : IObserver<IState>
    {
        /// <inheritdoc />
        public void OnCompleted()
        {
        }

        /// <inheritdoc />
        public void OnError(Exception error)
        {
        }

        /// <inheritdoc />
        public void OnNext(IState value)
        {
        }
    }
}