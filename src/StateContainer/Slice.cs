namespace StateContainer
{

    public class Slice<TState>(string name, TState initialState, Func<TState, IAction, TState> reducerLogic) : ISlice where TState : IState
    {
        public string Name { get; } = name;
        public IState InitialState { get; } = initialState;
        public Reducer ReducerFn { get; } = (state, action) =>
        {
            if (state is not TState typed)
            {
                throw new InvalidOperationException($"[Slice] Unexpected state type for slice '{name}'. Expected {typeof(TState).Name}.");
            }

            return reducerLogic(typed, action);
        };
    }
}
