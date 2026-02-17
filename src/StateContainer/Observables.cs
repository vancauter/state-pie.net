namespace StateContainer
{
    // Minimal Subject-like implementation with last-value semantics (Behavior-like)
    internal sealed class SimpleSubject<T>(T initial) : IObservable<T>, IDisposable
    {
        private readonly List<IObserver<T>> _observers = [];
        private bool _isDisposed;
        private T? _last = initial;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(SimpleSubject<T>));

            _observers.Add(observer);

            // replay last value if available
            if (_last is not null)
            {
                observer.OnNext(_last);
            }

            return new Unsubscriber(_observers, observer);
        }

        public void OnNext(T value)
        {
            if (_isDisposed) return;

            _last = value;
            foreach (var o in _observers.ToArray())
            {
                try { o.OnNext(value); } catch { }
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _observers.Clear();
        }

        private sealed class Unsubscriber(List<IObserver<T>> list, IObserver<T> observer) : IDisposable
        {
            private readonly List<IObserver<T>> _list = list;
            private readonly IObserver<T> _observer = observer;

            public void Dispose()
            {
                if (_list.Contains(_observer)) _list.Remove(_observer);
            }
        }
    }
}
