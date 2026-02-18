using AtomicRx.Core;

namespace AtomicRx
{
    public static partial class Factory
    {
        /// <summary>
        /// Create a typed <see cref="ISlice"/> with the provided name, initial state, and reducer.
        /// </summary>
        /// <typeparam name="TState">The concrete state type for the slice.</typeparam>
        /// <param name="name">The unique slice name.</param>
        /// <param name="initialState">Initial state instance for the slice.</param>
        /// <param name="reducer">Reducer function that computes next state for the slice.</param>
        /// <returns>An <see cref="ISlice"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or whitespace.</exception>
        public static ISlice CreateSlice<TState>(string name, TState initialState, Reducer<TState> reducer) where TState : IState
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(name));

            return new Slice<TState>(name, initialState, reducer);
        }
    }
}