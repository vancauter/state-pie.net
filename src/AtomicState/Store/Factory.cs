using System.Collections.ObjectModel;
using SC.AtomicState.Internal;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides factory helpers for constructing actions, slices, reducers, and stores.
    /// </summary>
    public static class Factory
    {
        /// <summary>
        /// Creates an <see cref="IAction"/> with the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type">Non-empty action type string.</param>
        /// <returns>An <see cref="IAction"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="type"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <remarks>
        /// Use stable action type strings so reducers and middleware can rely on predictable message names.
        /// </remarks>
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

        /// <summary>
        /// Creates a typed <see cref="ISlice"/> with the provided name, initial state, and reducer.
        /// </summary>
        /// <typeparam name="TState">The concrete state type for the slice.</typeparam>
        /// <param name="name">The unique slice name.</param>
        /// <param name="initialState">Initial state instance for the slice.</param>
        /// <param name="reducer">Reducer function that computes next state for the slice.</param>
        /// <returns>An <see cref="ISlice"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialState"/> or <paramref name="reducer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <remarks>
        /// The returned slice adapts the strongly typed reducer to the untyped <see cref="ISlice"/> contract used by <see cref="CombineReducers(ISlice[])"/>.
        /// Slice reducers should return the existing state instance when an action does not change that slice.
        /// </remarks>
        public static ISlice CreateSlice<TState>(string name, TState initialState, Reducer<TState> reducer) where TState : IState
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(name));
            }

            ArgumentNullException.ThrowIfNull(initialState);
            ArgumentNullException.ThrowIfNull(reducer);

            return new StateSlice<TState>(name, initialState, reducer);
        }

        /// <summary>
        /// Creates a root <see cref="IStore"/> with the provided initial state and root reducer.
        /// </summary>
        /// <param name="initialState">Initial root state.</param>
        /// <param name="rootReducer">A reducer that computes the next root state for each dispatched action.</param>
        /// <returns>An <see cref="IStore"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialState"/> or <paramref name="rootReducer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// The created store performs an internal initialization dispatch so reducers can materialize derived initial
        /// state before external subscribers observe the store. Initialization is part of store creation, so failures
        /// during that dispatch prevent the store from being created and are propagated unchanged to the caller.
        /// </remarks>
        public static IStore CreateStore(IState initialState, Reducer rootReducer)
        {
            ArgumentNullException.ThrowIfNull(initialState);
            ArgumentNullException.ThrowIfNull(rootReducer);

            return new Store(initialState, rootReducer);
        }

        /// <summary>
        /// Creates a root <see cref="IStore"/> with optional middleware that wraps dispatch.
        /// </summary>
        /// <param name="initialState">Initial root state.</param>
        /// <param name="rootReducer">Root reducer.</param>
        /// <param name="middleware">Optional middleware pipeline applied in order.</param>
        /// <returns>An <see cref="IStore"/> instance with middleware applied.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="initialState"/> or <paramref name="rootReducer"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// Middleware wraps the store's base dispatch pipeline in the order provided, so the first middleware receives
        /// each action first. Middleware receives an <see cref="IStore"/> handle for use by the returned dispatch
        /// delegate, but that handle cannot be accessed while the middleware pipeline is still being composed.
        /// Middleware may call <see cref="IStore.Dispatch(IAction)"/> again while a dispatch is already in progress,
        /// and that nested dispatch is serialized with other dispatch execution. Background work scheduled by
        /// middleware is outside the originating dispatch transaction, although any later dispatch from that work is
        /// still serialized by the store. Initialization is part of store creation, so failures during the internal
        /// initialization dispatch prevent the store from being created and are propagated unchanged to the caller.
        /// </remarks>
        public static IStore CreateStore(IState initialState, Reducer rootReducer, params Middleware[] middleware)
        {
            ArgumentNullException.ThrowIfNull(initialState);
            ArgumentNullException.ThrowIfNull(rootReducer);

            return new Store(initialState, rootReducer, middleware);
        }

        /// <summary>
        /// Combines multiple <see cref="ISlice"/> instances into a single root state and reducer.
        /// </summary>
        /// <param name="slices">An array of slices to combine. Each slice must be non-null and have a unique name.</param>
        /// <returns>
        /// A tuple containing the initial combined root state and a reducer that delegates to each slice reducer.
        /// Pass both tuple members forward together when creating a store for the combined state.
        /// </returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="slices"/> is empty, contains a null entry, or contains duplicate slice names.</exception>
        /// <remarks>
        /// The returned reducer only supports the combined root state shape created by this method. It preserves the
        /// existing root state reference when no slice produces a new state instance. That identity-preserving path is
        /// intentional because it prevents unnecessary state publication and avoids allocating a replacement slice map
        /// for actions that leave every slice unchanged.
        /// </remarks>
        public static (IState state, Reducer reducer) CombineReducers(params ISlice[] slices)
        {
            if (slices.Length == 0)
            {
                throw new ArgumentException("Must provide at least one slice to combine.", nameof(slices));
            }

            if (slices.Any(static slice => slice is null))
            {
                throw new ArgumentException("Slices cannot contain null entries.", nameof(slices));
            }

            var duplicateGroup = slices.GroupBy(slice => slice.Name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);

            if (duplicateGroup is not null)
            {
                throw new ArgumentException($"Duplicate slice name detected: {duplicateGroup.Key}", nameof(slices));
            }

            var reducerDictionary = slices.ToDictionary(slice => slice.Name, slice => slice.Reducer, StringComparer.Ordinal);
            var mutableSliceStates = slices.ToDictionary(slice => slice.Name, slice => slice.State, StringComparer.Ordinal);
            var initialSliceStates = new ReadOnlyDictionary<string, IState>(mutableSliceStates);
            var initialRootState = new CombinedState(initialSliceStates);

            IState rootReducer(IState state, IAction action)
            {
                if (state is not CombinedState currentRootState)
                {
                    throw new InvalidOperationException(
                        $"[CombineReducers] Unexpected state type: {state?.GetType().Name}. Expected {nameof(CombinedState)}.");
                }

                Dictionary<string, IState>? nextSliceStates = null;

                foreach (var sliceEntry in reducerDictionary)
                {
                    if (!currentRootState.States.TryGetValue(sliceEntry.Key, out var currentSliceState))
                    {
                        throw new InvalidOperationException(
                            $"[CombineReducers] Missing slice state for key: {sliceEntry.Key} in current root state.");
                    }

                    var nextSliceState = sliceEntry.Value(currentSliceState, action);
                    if (!ReferenceEquals(nextSliceState, currentSliceState))
                    {
                        // Clone only after the first real change so no-op actions can keep the current root snapshot
                        // without paying for a transient dictionary allocation.
                        nextSliceStates ??= new Dictionary<string, IState>(currentRootState.States, StringComparer.Ordinal);
                        nextSliceStates[sliceEntry.Key] = nextSliceState;
                    }
                }

                // Reuse the current root instance so reference-based change detection stays aligned with reducer
                // output and no synthetic root object is published for a logical no-op action.
                return nextSliceStates is null ? currentRootState : new CombinedState(nextSliceStates);
            }

            return (initialRootState, rootReducer);
        }
    }
}