namespace SCLibs.StateContainer.Core
{
    internal sealed partial class Store(IState initialState, ReducerFn rootReducer) : IStore
    {
        private bool _isDisposed;
        private readonly ReducerFn _rootReducer = rootReducer;
        // Use an internal subject implementation specialized for `IState`.
        private readonly StateSubject _stateSubject = new(initialState);

        // Observable stream of state changes.
        public IObservable<IState> StateChanged => _stateSubject;

        public IState State { get; private set; } = initialState;

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _stateSubject.Dispose();
        }

        public void Dispatch(IAction action)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(Store));

            var newState = _rootReducer(State, action);

            if (!ReferenceEquals(newState, State))
            {
                State = newState;
                _stateSubject.OnNext(State);
            }
        }
    }
}