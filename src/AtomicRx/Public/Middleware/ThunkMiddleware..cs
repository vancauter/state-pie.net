namespace AtomicRx
{
    public static class ThunkMiddleware
    {
        /// <summary>
        /// Thunk middleware: when an action carries a payload that is a function (thunk),
        /// invoke the payload instead of forwarding the action to reducers. This keeps the
        /// core Dispatch signature unchanged while enabling async/thunk flows via action payloads.
        ///
        /// Supported thunk shapes as action payloads:
        /// - <see cref="Func{IStore, Task}"/>: receives the store and can perform async work.
        /// - <see cref="Func{Dispatch, IStore, Task}"/>: receives the next dispatch function and the store.
        ///
        /// Note: The middleware invokes the thunk and does not await it on the dispatching thread.
        /// Callers should ensure exceptions are handled inside the thunk.
        /// </summary>
        public static Middleware Create() => (store, next) => action =>
        {
            // Handle IAction<Func<IStore, Task>>
            if (action is IAction<Func<IStore, Task>> thunkAction)
            {
                try
                {
                    _ = thunkAction.Payload(store);
                }
                catch
                {
                    // Swallow to avoid breaking the dispatch chain; thunks should handle their own errors.
                }
                return;
            }

            // Handle IAction<Func<Dispatch, IStore, Task>>
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

            // Default: forward to next middleware/reducer
            next(action);
        };
    }
}
