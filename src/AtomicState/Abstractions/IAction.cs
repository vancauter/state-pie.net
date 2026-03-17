namespace SC.AtomicState
{
    /// <summary>
    /// Represents a message dispatched to a store.
    /// </summary>
    /// <remarks>
    /// Actions are identified by their <see cref="Type"/> value and are expected to behave as immutable messages.
    /// </remarks>
    public interface IAction
    {
        /// <summary>
        /// Gets the semantic action type identifier.
        /// </summary>
        string Type { get; }
    }

    /// <summary>
    /// Represents an action that carries a strongly typed payload.
    /// </summary>
    /// <typeparam name="TPayload">The payload type for the action.</typeparam>
    /// <remarks>
    /// Payload values are exposed exactly as provided by the action factory or implementation.
    /// </remarks>
    public interface IAction<TPayload> : IAction
    {
        /// <summary>
        /// Gets the payload carried with the action.
        /// </summary>
        TPayload Payload { get; }
    }
}