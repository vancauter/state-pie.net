namespace SnapshotStore
{
    internal interface IStateSelectorSubscription
    {
        IStateSelectorUpdate Prepare(StateSnapshot state);

        void Deactivate();
    }

    internal interface IStateSelectorUpdate
    {
        Action? Commit();
    }

    /// <summary>
    /// Caches one selector's latest value and prepares atomic updates during store dispatch.
    /// </summary>
    internal sealed class StateSelectorSubscription<TValue> : IStateSelectorSubscription, IDisposable
    {
        private readonly Func<StateSnapshot, TValue> _selector;
        private readonly Action<TValue> _listener;
        private readonly IEqualityComparer<TValue> _comparer;
        private readonly Action<IStateSelectorSubscription> _unsubscribe;
        private TValue _currentValue;
        private bool _isActive = true;

        public StateSelectorSubscription(
            Func<StateSnapshot, TValue> selector,
            Action<TValue> listener,
            IEqualityComparer<TValue> comparer,
            StateSnapshot initialState,
            Action<IStateSelectorSubscription> unsubscribe)
        {
            _selector = selector;
            _listener = listener;
            _comparer = comparer;
            _unsubscribe = unsubscribe;
            _currentValue = selector(initialState);
        }

        public IStateSelectorUpdate Prepare(StateSnapshot state)
        {
            var nextValue = _selector(state);
            var hasChanged = !_comparer.Equals(_currentValue, nextValue);
            return new StateSelectorUpdate(this, nextValue, hasChanged);
        }

        public void Dispose() => _unsubscribe(this);

        public void Deactivate() => _isActive = false;

        private Action? Commit(TValue nextValue, bool hasChanged)
        {
            if (!_isActive)
                return null;

            // Advance the comparison baseline even when a custom comparer considers values equivalent.
            _currentValue = nextValue;
            return hasChanged ? () => _listener(nextValue) : null;
        }

        private sealed class StateSelectorUpdate : IStateSelectorUpdate
        {
            private readonly StateSelectorSubscription<TValue> _owner;
            private readonly TValue _nextValue;
            private readonly bool _hasChanged;

            public StateSelectorUpdate(StateSelectorSubscription<TValue> owner, TValue nextValue, bool hasChanged)
            {
                _owner = owner;
                _nextValue = nextValue;
                _hasChanged = hasChanged;
            }

            public Action? Commit() => _owner.Commit(_nextValue, _hasChanged);
        }
    }
}
