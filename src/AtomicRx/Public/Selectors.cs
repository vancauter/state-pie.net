using System.Threading;

namespace AtomicRx
{
    public static class Selectors
    {
        /// <summary>
        /// Create a thread-safe memoized selector function that caches the last
        /// input and result. The selector will re-evaluate the supplied
        /// <paramref name="selector"/> only when the comparer determines the
        /// input state has changed.
        /// </summary>
        /// <typeparam name="TState">Selector input state type.</typeparam>
        /// <typeparam name="TResult">Selector result type.</typeparam>
        /// <param name="selector">Function that computes the derived value from
        /// state.</param>
        /// <param name="comparer">Optional equality comparer for the state. If
        /// null, <see cref="EqualityComparer{T}.Default"/> is used.</param>
        /// <returns>A thread-safe memoized selector function.</returns>
        /// <remarks>
        /// Note: This uses "Single-Slot" memoization. If the input state
        /// oscillates between two different values, the selector will recompute
        /// on every call.
        /// </remarks>
        public static Func<TState, TResult> CreateMemoizedSelector<TState, TResult>(
            Func<TState, TResult> selector,
            IEqualityComparer<TState>? comparer = null)
        {
            // Use provided comparer or fall back to default equality comparer.
            comparer ??= EqualityComparer<TState>.Default;

            // Lightweight lock to protect updates to the cached state/result.
            var syncLock = new object();

            // Cache presence flag. We will use Volatile.Write when publishing
            // to ensure that once this becomes true other threads observing it
            // will also see the updated state/result values.
            var hasValue = false;

            // Cached inputs/outputs. Nullable to represent 'no value yet'.
            TState? lastState = default;
            TResult? lastResult = default;

            // Returned selector: attempts a fast-path read without locking,
            // computes the derived value outside the lock to avoid long blocking
            // critical sections, then reconciles/publishes under the lock. This
            // is the standard double-checked-locking (DCL) pattern adapted for
            // a single-slot memoizer.
            return state =>
            {
                // -----------------------------
                // Fast-path (no lock): if we
                // already have a cached value and
                // the comparer says the state is
                // equal, return the cached result.
                // -----------------------------
                if (hasValue && comparer.Equals(lastState!, state))
                {
                    // Return the previously computed result. We use the
                    // non-null assertion because hasValue guards cache presence.
                    return lastResult!;
                }

                // -----------------------------
                // Slow-path: cache miss. Compute
                // the selector result *outside* the
                // lock to avoid blocking other
                // threads while the compute occurs.
                // This may cause duplicate work if
                // multiple threads compute simultaneously,
                // which is an acceptable trade-off here.
                // -----------------------------
                var computed = selector(state);

                // -----------------------------
                // Reconcile and publish the computed
                // result under the lock. If another
                // thread already updated the cache to
                // the same state we will prefer that
                // value (it avoids replacing fresher data),
                // otherwise we publish our computed result.
                // -----------------------------
                lock (syncLock)
                {
                    // If cache is empty or out-of-date, store our computed
                    // result. We publish 'hasValue' after assigning the
                    // state/result to ensure proper publication ordering.
                    if (!hasValue || !comparer.Equals(lastState!, state))
                    {
                        lastState = state;
                        lastResult = computed;
                        // Ensure other threads observing 'hasValue' will also
                        // observe the updated lastState/lastResult.
                        Volatile.Write(ref hasValue, true);
                        return computed;
                    }

                    // Another thread updated the cache while we computed.
                    // Prefer the value stored by that thread as it is the
                    // canonical, up-to-date cache entry.
                    return lastResult!;
                }
            };
        }
    }
}
