namespace StatePie
{
    public static partial class StoreFactory
    {
        /// <summary>
        /// Factory method for creating a store from one or more slices, without any middleware or effects.
        /// </summary>
        /// <typeparam name="TAction">The type of actions the store will handle.</typeparam>
        /// <param name="slices">The slices to include in the store.</param>
        /// <returns>The created store instance.</returns>
        public static IStore<TAction> CreateStore<TAction>(params ISlice<TAction>[] slices)
            where TAction : class, IAction => BuildStore(
                slices,
                Array.Empty<DispatchMiddleware<TAction>>(),
                Array.Empty<Effect<TAction>>(),
                onEffectError: null);

        /// <summary>
        /// Factory method for creating a store from one or more slices, with synchronous middleware but without effects.
        /// </summary>
        /// <typeparam name="TAction">The type of actions the store will handle.</typeparam>
        /// <param name="slices">The slices to include in the store.</param>
        /// <param name="middleware">The synchronous middleware to wrap the store's dispatch.</param>
        /// <returns>The created store instance.</returns>
        /// <remarks>
        /// Middleware may short-circuit, exceptions propagate, and each middleware may invoke its next stage at most once.
        /// </remarks>
        public static IStore<TAction> CreateStore<TAction>(
            IReadOnlyList<ISlice<TAction>> slices,
            params DispatchMiddleware<TAction>[] middleware)
            where TAction : class, IAction =>
            CreateStore(slices, middleware, Array.Empty<Effect<TAction>>());

        /// <summary>
        /// Factory method for creating a store from one or more slices,
        /// with synchronous middleware and asynchronous effects.
        /// </summary>
        /// <typeparam name="TAction">The type of actions the store will handle.</typeparam>
        /// <param name="slices">The slices to include in the store.</param>
        /// <param name="middleware">The synchronous middleware to wrap the store's dispatch.</param>
        /// <param name="effects">The asynchronous effects to start for committed actions.</param>
        /// <param name="onEffectError">The optional error handler for faulted effect tasks.</param>
        /// <returns>The created store instance.</returns>
        /// <remarks>
        /// Effects start in registration order after reducers run and subscribers are notified,
        /// are not awaited by dispatch, report results by dispatching typed actions,
        /// and stop through a token cancelled on disposal, while faulted effect tasks go to the optional error handler
        /// and cancellation is not treated as an error.
        /// </remarks>
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

            return BuildStore(slices, middleware, effects, onEffectError);
        }
    }
}
