using System.Collections.ObjectModel;

namespace SnapshotStore
{
    /// <summary>
    /// Builds validated slices and stores so composition invariants are enforced once and runtime behavior stays consistent.
    /// </summary>
    public static class StoreFactory
    {
        /// <summary>
        /// Creates a typed slice wrapper that validates inputs and hides the internal adapter implementation.
        /// </summary>
        public static ISlice<TAction> CreateSlice<TState, TAction>(string name, TState initialState, Reducer<TState, TAction> reducer)
            where TState : IState
            where TAction : class, IAction
        {
            // Name validation is strict so state lookups are stable and diagnostics stay clear.
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(name));

            // Null initial state would fail unpredictably later during reducer execution.
            if (initialState is null)
                throw new ArgumentNullException(nameof(initialState));

            // Explicit delegate validation surfaces wiring errors at composition time.
            if (reducer is null)
                throw new ArgumentNullException(nameof(reducer));

            return new StoreSlice<TState, TAction>(name, initialState, reducer);
        }

        /// <summary>
        /// Creates a store from one or more slices that share the same action family while preserving compile-time type consistency.
        /// </summary>
        public static IStore<TAction> CreateStore<TAction>(params ISlice<TAction>[] slices)
            where TAction : class, IAction
        {
            // A store without slices has no meaningful behavior and usually indicates setup error.
            if (slices is null || slices.Length == 0)
                throw new ArgumentException("Must provide at least one slice to create a store.", nameof(slices));

            // Null slice entries hide bugs in composition call sites, so fail fast.
            if (slices.Any(static slice => slice is null))
                throw new ArgumentException("Slices cannot contain null entries.", nameof(slices));

            // Ordinal keying avoids culture-sensitive dictionary behavior for slice identifiers.
            var reducerDictionary = new Dictionary<string, Reducer<TAction>>(StringComparer.Ordinal);
            var initialSliceStates = new Dictionary<string, IState>(StringComparer.Ordinal);

            foreach (var slice in slices)
            {
                // Duplicate names would make state and reducer routing ambiguous.
                if (reducerDictionary.ContainsKey(slice.Name))
                    throw new ArgumentException($"Duplicate slice name detected: {slice.Name}", nameof(slices));

                reducerDictionary.Add(slice.Name, slice.Reducer);
                initialSliceStates.Add(slice.Name, slice.InitialState);
            }

            // The initial root snapshot is immutable so startup state cannot be changed externally.
            var initialRootState = new ReadOnlyDictionary<string, IState>(initialSliceStates);

            StateSnapshot rootReducerCore(StateSnapshot state, TAction action)
            {
                // Deferred allocation keeps no-op dispatches allocation-free.
                Dictionary<string, IState>? nextSliceStates = null;

                foreach (var pair in reducerDictionary)
                {
                    // Missing state indicates internal consistency break and should be explicit.
                    if (!state.TryGetValue(pair.Key, out var currentSliceState))
                        throw new InvalidOperationException($"Missing slice state for key '{pair.Key}'.");

                    var nextSliceState = pair.Value(currentSliceState, action);

                    // Identity check allows reducers to signal "no change" efficiently.
                    if (ReferenceEquals(nextSliceState, currentSliceState))
                        continue;

                    // Clone current root snapshot only when first change appears.
                    nextSliceStates ??= new Dictionary<string, IState>(StringComparer.Ordinal);
                    if (nextSliceStates.Count == 0)
                    {
                        foreach (var currentPair in state)
                            nextSliceStates.Add(currentPair.Key, currentPair.Value);
                    }

                    // Overwrite only changed slice to preserve untouched references.
                    nextSliceStates[pair.Key] = nextSliceState;
                }

                // Reuse existing root snapshot when nothing changed to avoid churn and preserve identity.
                return nextSliceStates is null ? state : new ReadOnlyDictionary<string, IState>(nextSliceStates);
            }

            return new Store<TAction>(initialRootState, rootReducerCore);
        }
    }
}