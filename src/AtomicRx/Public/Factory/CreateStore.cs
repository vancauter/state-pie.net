using AtomicRx.Core;

namespace AtomicRx
{
    public static partial class Factory
    {
        /// <summary>
        /// Create a root <see cref="IStore"/> with the provided initial state and root reducer.
        /// </summary>
        /// <param name="initialState">Initial root state. Typically created by <see cref="CombineReducers"/>.</param>
        /// <param name="rootReducer">A reducer that computes the next root state for each dispatched action.</param>
        /// <returns>An <see cref="IStore"/> instance.</returns>
        public static IStore CreateStore(IState initialState, Reducer rootReducer) => new Store(initialState, rootReducer);

        /// <summary>
        /// Create a root <see cref="IStore"/> with optional middleware that wraps dispatch.
        /// </summary>
        /// <param name="initialState">Initial root state.</param>
        /// <param name="rootReducer">Root reducer.</param>
        /// <param name="middleware">Optional middleware pipeline applied in order.</param>
        /// <returns>An <see cref="IStore"/> instance with middleware applied.</returns>
        public static IStore CreateStore(IState initialState, Reducer rootReducer, params Middleware[] middleware)
            => new Store(initialState, rootReducer, middleware);
    }
}
