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
        public IState State { get; } = initialState;

        /// <inheritdoc />
        public Reducer Reducer { get; } = (state, action) => reducer((TState)state, action);
    }
}