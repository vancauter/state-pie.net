namespace AtomicRx.Runtime
{
    /// <summary>
    /// Represents a root state composed from named slice states.
    /// </summary>
    internal sealed class CombinedState(IReadOnlyDictionary<string, IState> sliceStates) : IState
    {
        /// <summary>
        /// Gets the slice states stored in the combined root state.
        /// </summary>
        internal IReadOnlyDictionary<string, IState> States { get; } = sliceStates;
    }
}