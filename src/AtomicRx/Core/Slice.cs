namespace AtomicRx.Core
{
    // Internal implementation of an ISlice.
    // A lightweight container holding the slice name, the current state instance and a reducer delegate.
    internal sealed class Slice<TState>(string name, TState initialState, Reducer<TState> reducer) : ISlice where TState : IState
    {
        // The unique name identifying this slice in a combined root state.
        public string Name { get; } = name;

        // The slice's current state instance. Note: reducers are expected to treat states as immutable.
        public IState State { get; } = initialState;

        // Adapter that exposes the generic reducer as the non-generic Reducer required by ISlice.
        // It casts the incoming IState back to TState before invoking the typed reducer.
        public Reducer Reducer { get; } = (state, action) => reducer((TState)state, action);
    }
}