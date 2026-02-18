namespace AtomicRx
{
    public static partial class Factory
    {
        /// <summary>
        /// Create a simple <see cref="IAction"/> with the specified <paramref name="type"/>.
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
        /// Create an <see cref="IAction{TPayload}"/> carrying a strongly-typed payload.
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

        /// <summary>
        /// Create a typed action creator function that produces an <see cref="IAction{TPayload}"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <typeparam name="TPayload">Payload type produced by the action creator.</typeparam>
        /// <param name="type">The action type string.</param>
        /// <returns>A function that accepts a payload and returns an <see cref="IAction{TPayload}"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is null or whitespace.</exception>
        /// <example>
        /// <code>
        /// var addTodo = Factory.CreateActionCreator<string>("ADD_TODO");
        /// var action = addTodo("Buy milk"); // Creates an IAction<string> with Type = "ADD_TODO" and Payload = "Buy milk"
        /// </code>
        /// </example>
        public static Func<TPayload, IAction<TPayload>> CreateActionCreator<TPayload>(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));

            return payload => new Core.Action<TPayload>(type, payload);
        }
    }
}