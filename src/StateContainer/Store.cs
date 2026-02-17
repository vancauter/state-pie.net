namespace StateContainer
{
    public class StateContainer(IState initialState, Reducer rootReducer) : IStateContainer
    {
        private readonly Reducer _rootReducer = rootReducer;

        private readonly SimpleSubject<IState> _stateSubject = new(initialState);

        public IObservable<IState> StateChanged => _stateSubject;

        public IState State { get; private set; } = initialState;

        public void Dispose()
        {
            _stateSubject.Dispose();
            GC.SuppressFinalize(this);
        }

        public void Dispatch(IAction action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var newState = _rootReducer(State, action);

            // TODO: Replace Console.WriteLine with proper logging in production
            Console.WriteLine($"[StateContainer] Dispatched action: {action.Type}");

            if (!ReferenceEquals(newState, State))
            {
                Console.WriteLine($"[StateContainer] New state: {newState}");

                State = newState;
                _stateSubject.OnNext(State);
            }
        }
    }
}
