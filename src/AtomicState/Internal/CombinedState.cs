namespace SC.AtomicState.Internal
{
    /// <summary>
    /// A compact immutable holder for the combined root state composed from named slice states.
    /// </summary>
    /// <remarks>
    /// This type is internal and used by <see cref="Factory"/> when combining slice reducers
    /// into a single root state. The `States` dictionary contains each slice state keyed by its slice name.
    /// </remarks>
    internal sealed record CombinedState(IReadOnlyDictionary<string, IState> States) : IState;
}