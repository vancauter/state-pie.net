using SC.AtomicState.Runtime;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides factory helpers for constructing actions, slices, reducers, and stores.
    /// </summary>
    public static partial class Factory
    {
        /// <summary>
        /// Creates a root <see cref="IStore"/> with the provided initial state and root reducer.
        /// </summary>
        /// <param name="initialState">Initial root state.</param>
        /// <param name="rootReducer">A reducer that computes the next root state for each dispatched action.</param>
        /// <returns>An <see cref="IStore"/> instance.</returns>
        /// <remarks>
        /// The created store performs an internal initialization dispatch so reducers can materialize derived initial
        /// state before external subscribers observe the store. Initialization is part of store creation, so failures
        /// during that dispatch prevent the store from being created and are propagated unchanged to the caller.
        /// </remarks>
        public static IStore CreateStore(IState initialState, Reducer rootReducer) => new Store(initialState, rootReducer);

        /// <summary>
        /// Creates a root <see cref="IStore"/> with optional middleware that wraps dispatch.
        /// </summary>
        /// <param name="initialState">Initial root state.</param>
        /// <param name="rootReducer">Root reducer.</param>
        /// <param name="middleware">Optional middleware pipeline applied in order.</param>
        /// <returns>An <see cref="IStore"/> instance with middleware applied.</returns>
        /// <remarks>
        /// Middleware wraps the store's base dispatch pipeline in the order provided, so the first middleware receives
        /// each action first. Middleware may call <see cref="IStore.Dispatch(IAction)"/> again while a dispatch is
        /// already in progress, and that nested dispatch is serialized with other dispatch execution. Background work
        /// scheduled by middleware is outside the originating dispatch transaction, although any later dispatch from
        /// that work is still serialized by the store. Initialization is part of store creation, so failures during
        /// the internal initialization dispatch prevent the store from being created and are propagated unchanged to
        /// the caller.
        /// </remarks>
        public static IStore CreateStore(IState initialState, Reducer rootReducer, params AtomicMiddleware[] middleware) => new Store(initialState, rootReducer, middleware);
    }
}