namespace SCLibs.StateContainer
{
    internal sealed class SimpleSubject<T>(T initial) : IObservable<T>, IDisposable
    {
        private readonly List<IObserver<T>> _observers = [];
        private readonly object _lock = new();
        private bool _isDisposed;
        private T? _last = initial;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(SimpleSubject<T>));

            T? last;
            lock (_lock)
            {
                _observers.Add(observer);
                last = _last;
            }

            // Replay last value outside lock to avoid re-entrance issues.
            if (last is not null)
            {
                try { observer.OnNext(last); } catch { }
            }

            return new Subscription(() => { lock (_lock) { _ = _observers.RemoveAll(o => o == observer); } });
        }

        public void OnNext(T value)
        {
            if (_isDisposed) return;

            IObserver<T>[] snapshot;
            lock (_lock)
            {
                _last = value;
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

        private sealed class Subscription(System.Action disposeAction) : IDisposable
        {
            private readonly System.Action _disposeAction = disposeAction;

            public void Dispose() => _disposeAction();
        }
    }
}
