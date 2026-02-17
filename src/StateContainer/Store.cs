namespace SCLibs.StateContainer
{
    public class StateContainer(IState initialState, Reducer rootReducer) : IStateContainer
    {
        private bool _isDisposed;
        private readonly Reducer _rootReducer = rootReducer;

        private readonly SimpleSubject<IState> _stateSubject = new(initialState);

        public IObservable<IState> StateChanged => _stateSubject;

        public IState State { get; private set; } = initialState;

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;
            _stateSubject.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispatch(IAction action)
        {
            ObjectDisposedException.ThrowIf(_isDisposed, nameof(StateContainer));

            var newState = _rootReducer(State, action);

            if (!ReferenceEquals(newState, State))
            {
                State = newState;
                _stateSubject.OnNext(State);
            }
        }
    }
}
