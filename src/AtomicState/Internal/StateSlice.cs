namespace SC.AtomicState.Internal
{
    /// <summary>
    /// Adapts a strongly typed slice definition to the untyped <see cref="ISlice"/> contract.
    /// </summary>
    internal sealed class StateSlice<TState>(string name, TState initialState, Reducer<TState> reducer) : ISlice where TState : IState
    {
        /// <inheritdoc />
        public string Name { get; } = name;

        /// <inheritdoc />
        public IState InitialState { get; } = initialState;

        /// <inheritdoc />
        public Reducer Reducer { get; } = (state, action) =>
            // Combined reducers store slice snapshots through the IState surface, so adapt back to the concrete slice
            // type at the last possible boundary.
            reducer((TState)state, action);
    }
}