using System.Collections.ObjectModel;

namespace StatePie
{
    public static partial class StoreFactory
    {
        /// <summary>
        /// Factory method for creating a slice.
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

            // Creates a typed slice wrapper
            return new StoreSlice<TState, TAction>(name, initialState, reducer);
        }

        /// <summary>
        /// Factory method for creating a store from one or more slices.
        /// </summary>
        public static IStore<TAction> CreateStore<TAction>(params ISlice<TAction>[] slices)
            where TAction : class, IAction => CreateStoreCore(
                slices,
                Array.Empty<DispatchMiddleware<TAction>>(),
                Array.Empty<Effect<TAction>>(),
                onEffectError: null);

        /// <summary>
        /// Creates a store whose typed dispatch is wrapped by synchronous middleware in registration order; middleware may short-circuit, exceptions propagate, and each middleware may invoke its next stage at most once.
        /// </summary>
        public static IStore<TAction> CreateStore<TAction>(
            IReadOnlyList<ISlice<TAction>> slices,
            params DispatchMiddleware<TAction>[] middleware)
            where TAction : class, IAction =>
            CreateStore(slices, middleware, Array.Empty<Effect<TAction>>());

        /// <summary>
        /// Creates a store that also starts asynchronous effects for committed actions; effects start in registration order after reducers run and subscribers are notified, are not awaited by dispatch, report results by dispatching typed actions, and stop through a token cancelled on disposal, while faulted effect tasks go to the optional error handler and cancellation is not treated as a fault.
        /// </summary>
        public static IStore<TAction> CreateStore<TAction>(
            IReadOnlyList<ISlice<TAction>> slices,
            IReadOnlyList<DispatchMiddleware<TAction>> middleware,
            IReadOnlyList<Effect<TAction>> effects,
            Action<Exception, TAction>? onEffectError = null)
            where TAction : class, IAction
        {
            if (middleware is null)
                throw new ArgumentNullException(nameof(middleware));

            if (middleware.Any(static item => item is null))
                throw new ArgumentException("Middleware cannot contain null entries.", nameof(middleware));

            if (effects is null)
                throw new ArgumentNullException(nameof(effects));

            if (effects.Any(static item => item is null))
                throw new ArgumentException("Effects cannot contain null entries.", nameof(effects));

            return CreateStoreCore(slices, middleware, effects, onEffectError);
        }

        private static IStore<TAction> CreateStoreCore<TAction>(
            IEnumerable<ISlice<TAction>> slices,
            IReadOnlyList<DispatchMiddleware<TAction>> middleware,
            IReadOnlyList<Effect<TAction>> effects,
            Action<Exception, TAction>? onEffectError)
            where TAction : class, IAction
        {
            // A store without slices has no meaningful behavior and usually indicates setup error.
            if (slices is null)
                throw new ArgumentException("Must provide at least one slice to create a store.", nameof(slices));

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

            // Return a new store instance initialized with the composed root state, reducer, middleware, and effects.
            return new Store<TAction>(initialRootState, rootReducerCore, middleware, effects, onEffectError);
        }
    }
}
