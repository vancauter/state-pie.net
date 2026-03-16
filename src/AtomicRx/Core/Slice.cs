namespace AtomicRx.Core
{
    /// <summary>
    /// Represents the internal <see cref="ISlice"/> implementation.
    /// </summary>
    internal sealed class Slice<TState>(string name, TState initialState, Reducer<TState> reducer) : ISlice where TState : IState
    {
        /// <summary>
        /// Gets the slice name.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the current slice state.
        /// </summary>
        public IState State { get; } = initialState;

        /// <summary>
        /// Gets the reducer adapter exposed through <see cref="ISlice"/>.
        /// </summary>
        public Reducer Reducer { get; } = (state, action) => reducer((TState)state, action);
    }
}