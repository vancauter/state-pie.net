namespace SC.AtomicState
{
    /// <summary>
    /// Represents a message dispatched to a store.
    /// </summary>
    /// <remarks>
    /// Actions are identified by their <see cref="Type"/> value and are expected to behave as immutable messages.
    /// Reducers and middleware typically branch on the action type rather than the concrete action implementation.
    /// </remarks>
    public interface IAction
    {
        /// <summary>
        /// Gets the semantic action type identifier.
        /// </summary>
        /// <remarks>
        /// Factory-created actions require a non-empty type string so dispatch consumers can rely on predictable
        /// message names.
        /// </remarks>
        string Type { get; }
    }

    /// <summary>
    /// Represents an action that carries a strongly typed payload.
    /// </summary>
    /// <typeparam name="TPayload">The payload type for the action.</typeparam>
    /// <remarks>
    /// Payload values are exposed exactly as provided by the action factory or implementation.
    /// The payload is not copied, transformed, or validated by the action contract.
    /// </remarks>
    public interface IAction<TPayload> : IAction
    {
        /// <summary>
        /// Gets the payload carried with the action.
        /// </summary>
        TPayload Payload { get; }
    }
}