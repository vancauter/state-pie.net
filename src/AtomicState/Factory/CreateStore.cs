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
        /// The created store performs an internal initialization dispatch so reducers can materialize derived initial state before external subscribers observe the store.
        /// </remarks>
        public static IStore CreateStore(IState initialState, Reducer rootReducer) => new StoreRuntime(initialState, rootReducer);

        /// <summary>
        /// Creates a root <see cref="IStore"/> with optional middleware that wraps dispatch.
        /// </summary>
        /// <param name="initialState">Initial root state.</param>
        /// <param name="rootReducer">Root reducer.</param>
        /// <param name="middleware">Optional middleware pipeline applied in order.</param>
        /// <returns>An <see cref="IStore"/> instance with middleware applied.</returns>
        /// <remarks>
        /// Middleware wraps the store's base dispatch pipeline in the order provided, so the first middleware receives each action first.
        /// </remarks>
        public static IStore CreateStore(IState initialState, Reducer rootReducer, params AtomicMiddleware[] middleware) => new StoreRuntime(initialState, rootReducer, middleware);
    }
}