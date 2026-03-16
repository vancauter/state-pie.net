namespace AtomicRx
{
    /// <summary>
    /// Provides selector helpers for deriving values from state.
    /// </summary>
    public static class Selectors
    {
        /// <summary>
        /// Creates a thread-safe memoized selector that caches the last input and result.
        /// </summary>
        /// <typeparam name="TState">Selector input state type.</typeparam>
        /// <typeparam name="TResult">Selector result type.</typeparam>
        /// <param name="selector">Function that computes the derived value from state.</param>
        /// <param name="comparer">Optional equality comparer for the state. When <see langword="null"/>, <see cref="EqualityComparer{T}.Default"/> is used.</param>
        /// <returns>A thread-safe memoized selector function.</returns>
        /// <remarks>
        /// This method performs single-entry memoization. If successive inputs compare equal, the cached result is reused.
        /// </remarks>
        public static Func<TState, TResult> CreateMemoizedSelector<TState, TResult>(
            Func<TState, TResult> selector,
            IEqualityComparer<TState>? comparer = null)
        {
            comparer ??= EqualityComparer<TState>.Default;
            var syncLock = new Lock();
            var hasValue = false;
            TState? lastState = default;
            TResult? lastResult = default;

            return state =>
            {
                if (Volatile.Read(ref hasValue) && comparer.Equals(lastState!, state))
                {
                    return lastResult!;
                }

                var computed = selector(state);

                lock (syncLock)
                {
                    if (!hasValue || !comparer.Equals(lastState!, state))
                    {
                        lastState = state;
                        lastResult = computed;
                        Volatile.Write(ref hasValue, true);
                        return computed;
                    }

                    return lastResult!;
                }
            };
        }
    }
}