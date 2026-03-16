namespace AtomicRx
{
    /// <summary>
    /// Represents an action dispatched to a store.
    /// </summary>
    /// <remarks>
    /// Actions are identified by their <see cref="Type"/> value and should be treated as immutable messages.
    /// </remarks>
    public interface IAction
    {
        /// <summary>
        /// Gets the action type identifier.
        /// </summary>
        string Type { get; }
    }

    /// <summary>
    /// Represents an action that carries a strongly typed payload.
    /// </summary>
    /// <typeparam name="TPayload">The payload type for the action.</typeparam>
    public interface IAction<TPayload> : IAction
    {
        /// <summary>
        /// Gets the payload carried with the action.
        /// </summary>
        TPayload Payload { get; }
    }
}