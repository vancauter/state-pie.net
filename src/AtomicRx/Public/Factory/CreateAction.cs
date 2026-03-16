namespace AtomicRx
{
    /// <summary>
    /// Provides factory helpers for constructing actions, slices, reducers, and stores.
    /// </summary>
    public static partial class Factory
    {
        /// <summary>
        /// Creates an <see cref="IAction"/> with the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Non-empty action type string.</param>
        /// <returns>An <see cref="IAction"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is null or whitespace.</exception>
        public static IAction CreateAction(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new Core.Action(type);
        }

        /// <summary>
        /// Creates an <see cref="IAction{TPayload}"/> carrying a strongly typed payload.
        /// </summary>
        /// <typeparam name="TPayload">Payload type.</typeparam>
        /// <param name="type">Non-empty action type string.</param>
        /// <param name="payload">Payload instance to attach to the action.</param>
        /// <returns>An <see cref="IAction{TPayload}"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is null or whitespace.</exception>
        public static IAction CreateAction<TPayload>(string type, TPayload payload)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return new Core.Action<TPayload>(type, payload);
        }

        // Action-creator helpers live in separate record types so the core factory remains focused on value construction.
    }
}