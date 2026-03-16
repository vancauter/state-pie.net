namespace AtomicRx.Runtime
{
    /// <summary>
    /// Represents a root state composed from named slice states.
    /// </summary>
    internal sealed class CombinedState : IState
    {
        /// <summary>
        /// Initializes a new combined state instance.
        /// </summary>
        internal CombinedState(IReadOnlyDictionary<string, IState> sliceStates) => States = sliceStates;

        /// <summary>
        /// Gets the slice states stored in the combined root state.
        /// </summary>
        internal IReadOnlyDictionary<string, IState> States { get; }

        /// <summary>
        /// Creates a combined state from an existing read-only slice map.
        /// </summary>
        internal static CombinedState Create(IReadOnlyDictionary<string, IState> sliceStates) => new(sliceStates);
    }
}