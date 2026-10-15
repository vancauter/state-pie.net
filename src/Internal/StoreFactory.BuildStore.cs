using System.Collections.ObjectModel;

namespace StatePie
{
    public static partial class StoreFactory
    {
        /// <summary>
        /// Creates a store instance from the provided slices, middleware, and effects.
        /// </summary>
        private static IStore<TAction> BuildStore<TAction>(
            IEnumerable<ISlice<TAction>> slices,
            IReadOnlyList<DispatchMiddleware<TAction>> middleware,
            IReadOnlyList<Effect<TAction>> effects,
            Action<Exception, TAction>? onEffectError)
            where TAction : class, IAction
        {
            // A store without slices has no meaningful behavior, so enforce at least one slice.
            if (slices is null)
                throw new ArgumentException("Must provide at least one slice to create a store.", nameof(slices));

            // Convert slices to an array to allow multiple enumerations and enforce non-empty collection.
            var sliceArray = slices.ToArray();
            if (sliceArray.Length == 0)
                throw new ArgumentException("Must provide at least one slice to create a store.", nameof(slices));

            // Null slice entries hide bugs in composition call sites, so fail fast.
            if (sliceArray.Any(static slice => slice is null))
                throw new ArgumentException("Slices cannot contain null entries.", nameof(slices));

            // Ordinal keying avoids culture-sensitive dictionary behavior for slice identifiers.
            var reducerDictionary = new Dictionary<string, Reducer<TAction>>(StringComparer.Ordinal);
            var initialSliceStates = new Dictionary<string, IState>(StringComparer.Ordinal);

            foreach (var slice in sliceArray)
            {
                // Duplicate names make state and reducer routing ambiguous, so fail fast.
                if (reducerDictionary.ContainsKey(slice.Name))
                    throw new ArgumentException($"Duplicate slice name detected: {slice.Name}", nameof(slices));

                reducerDictionary.Add(slice.Name, slice.Reducer);
                initialSliceStates.Add(slice.Name, slice.InitialState);
            }

            // The initial root snapshot is immutable so startup state cannot be changed externally.
            var initialRootState = new ReadOnlyDictionary<string, IState>(initialSliceStates);

            // The root reducer core handles state transitions by delegating to individual slice reducers.
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

            // Return a new store instance initialized with the composed root state, reducer, middleware, and effects.
            return new Store<TAction>(initialRootState, rootReducerCore, middleware, effects, onEffectError);
        }
    }
}
