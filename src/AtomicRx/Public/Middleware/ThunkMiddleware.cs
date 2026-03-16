namespace AtomicRx
{
    /// <summary>
    /// Provides middleware that executes thunk payloads instead of forwarding them to reducers.
    /// </summary>
    /// <remarks>
    /// Supported thunk payload shapes are <see cref="Action{IStore}"/>, <see cref="Action{Dispatch, IStore}"/>, <see cref="Func{IStore, Task}"/>, and <see cref="Func{Dispatch, IStore, Task}"/>.
    /// Async thunks are started without awaiting them on the dispatching thread, and synchronous invocation failures are suppressed to keep dispatch moving.
    /// </remarks>
    public static class ThunkMiddleware
    {
        /// <summary>
        /// Creates thunk middleware.
        /// </summary>
        /// <returns>A middleware delegate that intercepts supported thunk payloads.</returns>
        public static Middleware Create() => (store, next) => action =>
        {
            if (action is IAction<Action<IStore>> syncThunkAction)
            {
                try
                {
                    syncThunkAction.Payload(store);
                }
                catch
                {
                    // Swallow to avoid breaking the dispatch chain; thunks should handle their own errors.
                }
                return;
            }

            if (action is IAction<Action<Dispatch, IStore>> syncThunkWithNext)
            {
                try
                {
                    syncThunkWithNext.Payload(next, store);
                }
                catch
                {
                }
                return;
            }

            if (action is IAction<Func<IStore, Task>> thunkAction)
            {
                try
                {
                    _ = thunkAction.Payload(store);
                }
                catch
                {
                }
                return;
            }

            if (action is IAction<Func<Dispatch, IStore, Task>> thunkActionWithNext)
            {
                try
                {
                    _ = thunkActionWithNext.Payload(next, store);
                }
                catch
                {
                }
                return;
            }

            next(action);
        };
    }
}
