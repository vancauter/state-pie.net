namespace SCLibs.StateContainer.Core
{
    internal sealed class Slice<TState>(string name, TState initialState, ReducerFn<TState> reducer) : ISlice where TState : IState
    {
        public string Name { get; } = name;
        public IState State { get; } = initialState;
        public ReducerFn Reducer { get; } = (state, action) => reducer((TState)state, action);
    }
}