namespace AtomicRx.Core
{
    /// <summary>
    /// Represents the internal non-generic action implementation.
    /// </summary>
    internal sealed record Action(string Type) : IAction;

    /// <summary>
    /// Represents the internal payload action implementation.
    /// </summary>
    internal sealed record Action<TPayload>(string Type, TPayload Payload) : IAction<TPayload>;
}