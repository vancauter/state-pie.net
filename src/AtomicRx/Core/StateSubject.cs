namespace SCLibs.StateContainer.Core
{
    internal sealed class StateSubject(IState initialState) : IObservable<IState>, IDisposable
    {
        private readonly List<IObserver<IState>> _observers = [];
        private readonly object _lock = new();
        private bool _isDisposed;
        private IState? _lastState = initialState;

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

            return new Subscription(() => { lock (_lock) { _ = _observers.Remove(observer); } });
        }

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

        private sealed class Subscription(System.Action disposeAction) : IDisposable
        {
            private readonly System.Action _disposeAction = disposeAction;

            public void Dispose() => _disposeAction();
        }
    }
}
