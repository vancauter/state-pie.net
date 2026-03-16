using AtomicRx.Runtime;

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
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is <see langword="null"/>, empty, or whitespace.</exception>
        public static IAction CreateAction(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));
            }

            return new StoreAction(type);
        }

        /// <summary>
        /// Creates an <see cref="IAction{TPayload}"/> carrying a strongly typed payload.
        /// </summary>
        /// <typeparam name="TPayload">Payload type.</typeparam>
        /// <param name="type">Non-empty action type string.</param>
        /// <param name="payload">Payload instance to attach to the action.</param>
        /// <returns>An <see cref="IAction{TPayload}"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <remarks>
        /// The payload is stored as supplied and is not copied or transformed.
        /// </remarks>
        public static IAction<TPayload> CreateAction<TPayload>(string type, TPayload payload)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentException("Action type must be a non-empty string.", nameof(type));
            }

            return new StoreAction<TPayload>(type, payload);
        }
    }
}