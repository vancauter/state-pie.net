namespace SC.AtomicState.Internal
{
    /// <summary>
    /// Represents a combined root state composed from named slice states.
    /// </summary>
    internal sealed record CombinedState(IReadOnlyDictionary<string, IState> States) : IState;
}