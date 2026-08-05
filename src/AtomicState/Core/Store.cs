using System.Collections.Generic;
using System;
using System.Threading;

namespace AtomicState
{
    internal sealed class Store : IStore
    {
        private bool _isDisposed;
        private readonly object _lock = new object();
        private readonly RootReducer _reducer;
        private IReadOnlyDictionary<string, IState> _state;

        public Store(IReadOnlyDictionary<string, IState> initialState, RootReducer reducer)
        {
            if (initialState is null)
                throw new ArgumentNullException(nameof(initialState));

            if (reducer is null)
                throw new ArgumentNullException(nameof(reducer));

            _state = initialState;
            _reducer = reducer;
        }

        public event System.Action<IReadOnlyDictionary<string, IState>> OnStateChanged;

        public IReadOnlyDictionary<string, IState> State => Volatile.Read(ref _state);

        public void Dispatch(IAction action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            System.Action<IReadOnlyDictionary<string, IState>> handlers = null;
            IReadOnlyDictionary<string, IState> nextState;

            lock (_lock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(Store));

                var currentState = _state;
                nextState = _reducer(currentState, action);
                if (ReferenceEquals(nextState, currentState))
                    return;

                Volatile.Write(ref _state, nextState);
                handlers = OnStateChanged;
            }

            handlers?.Invoke(nextState);
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                OnStateChanged = null;
            }
        }
    }
}