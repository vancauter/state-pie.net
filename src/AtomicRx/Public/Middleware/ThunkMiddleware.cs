namespace AtomicRx
{
    /// <summary>
    /// Thunk middleware: when an action carries a payload that is a function (thunk),
    /// invoke the payload instead of forwarding the action to reducers. This keeps the
    /// core Dispatch signature unchanged while enabling async/thunk flows via action payloads.
    ///
    /// Supported thunk shapes as action payloads:
    /// - <see cref="Func{IStore, Task}"/>, <see cref="Action{IStore}"/>: receives the store and can perform work.
    /// - <see cref="Func{Dispatch, IStore, Task}"/>, <see cref="Action{Dispatch, IStore}"/>: receives the next dispatch function and the store.
    ///
    /// Note: The middleware invokes thunks and will not await async thunks on the dispatching thread.
    /// Callers should ensure exceptions are handled inside the thunk if they don't want them swallowed.
    /// </summary>
    public static class ThunkMiddleware
    {
        public static Middleware Create() => (store, next) => action =>
        {
            // Handle IAction<Action<IStore>>
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

            // Handle IAction<Action<Dispatch, IStore>>
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

            // Handle IAction<Func<IStore, Task>>
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
