namespace SCLibs.StateContainer
{
    internal class Slice<TState>(string name, TState initialState, ReducerFn<TState> reducer) : ISlice where TState : IState
    {
        public string Name { get; } = name;
        public IState State { get; } = initialState;
        // Exposes a non-generic `ReducerFn` that internally casts `IState` to `TState`.
        // Reducers should return the same instance when state is unchanged (reference-equality).
        public ReducerFn Reducer { get; } = (state, action) => reducer((TState)state, action);
    }
}
