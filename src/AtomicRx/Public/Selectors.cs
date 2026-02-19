namespace AtomicRx
{
    public static class Selectors
    {
        /// <summary>
        /// Create a memoized selector function that caches the last input and result.
        /// The selector will re-evaluate the supplied <paramref name="selector"/> only when
        /// the comparer determines the input state changed.
        /// </summary>
        /// <typeparam name="TState">Selector input state type.</typeparam>
        /// <typeparam name="TResult">Selector result type.</typeparam>
        /// <param name="selector">Function that computes the derived value from state.</param>
        /// <param name="comparer">Optional equality comparer for the state. If null, <see cref="EqualityComparer{T}.Default"/> is used.</param>
        /// <returns>A memoized selector function.</returns>
        public static Func<TState, TResult> CreateMemoizedSelector<TState, TResult>(Func<TState, TResult> selector, IEqualityComparer<TState>? comparer = null)
        {
            comparer ??= EqualityComparer<TState>.Default;

            var hasValue = false;
            TState? lastState = default;
            TResult? lastResult = default;

            return state =>
            {
                if (hasValue && comparer.Equals(lastState!, state))
                {
                    return lastResult!;
                }

                var res = selector(state);
                lastState = state;
                lastResult = res;
                hasValue = true;
                return res;
            };
        }
    }
}
