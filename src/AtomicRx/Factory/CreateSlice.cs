using AtomicRx.Runtime;

namespace AtomicRx
{
    /// <summary>
    /// Provides factory helpers for constructing actions, slices, reducers, and stores.
    /// </summary>
    public static partial class Factory
    {
        /// <summary>
        /// Creates a typed <see cref="ISlice"/> with the provided name, initial state, and reducer.
        /// </summary>
        /// <typeparam name="TState">The concrete state type for the slice.</typeparam>
        /// <param name="name">The unique slice name.</param>
        /// <param name="initialState">Initial state instance for the slice.</param>
        /// <param name="reducer">Reducer function that computes next state for the slice.</param>
        /// <returns>An <see cref="ISlice"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <remarks>
        /// The returned slice adapts the strongly typed reducer to the untyped <see cref="ISlice"/> contract used by <see cref="CombineReducers(ISlice[])"/>.
        /// </remarks>
        public static ISlice CreateSlice<TState>(string name, TState initialState, Reducer<TState> reducer) where TState : IState
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(name));
            }

            return new StateSlice<TState>(name, initialState, reducer);
        }
    }
}