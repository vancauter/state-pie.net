namespace AtomicRx
{
    /// <summary>
    /// Provides middleware that executes supported thunk payloads instead of forwarding them to reducers.
    /// </summary>
    /// <remarks>
    /// Supported payload shapes are <see cref="Action{T}"/> with <see cref="IStore"/>, <see cref="Action{T1, T2}"/> with <see cref="Dispatch"/> and <see cref="IStore"/>, and their <see cref="Task"/>-returning counterparts.
    /// Thunk invocation failures are suppressed so middleware does not terminate the dispatch pipeline.
    /// </remarks>
    public static class ThunkMiddleware
    {
        /// <summary>
        /// Creates thunk middleware.
        /// </summary>
        /// <returns>A middleware delegate that intercepts supported thunk payloads.</returns>
        /// <remarks>
        /// Async thunks are started without awaiting them on the caller thread.
        /// </remarks>
        public static AtomicMiddleware Create() => (store, next) => action =>
        {
            if (action is IAction<Action<IStore>> syncThunkAction)
            {
                try
                {
                    syncThunkAction.Payload(store);
                }
                catch
                {
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