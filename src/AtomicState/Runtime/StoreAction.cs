namespace SC.AtomicState.Runtime
{
    /// <summary>
    /// Represents the internal action implementation without a payload.
    /// </summary>
    internal sealed record StoreAction(string Type) : IAction;

    /// <summary>
    /// Represents the internal action implementation with a payload.
    /// </summary>
    internal sealed record StoreAction<TPayload>(string Type, TPayload Payload) : IAction<TPayload>;
}