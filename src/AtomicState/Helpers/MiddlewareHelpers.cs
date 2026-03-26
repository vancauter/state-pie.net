namespace SC.AtomicState.Helpers
{
    /// <summary>
    /// Helper utilities for middleware authors to schedule background work and perform safe async dispatches.
    /// </summary>
    public static class MiddlewareHelpers
    {
        /// <summary>
        /// Schedules the provided work on the thread-pool and ensures a cooperative yield before executing.
        /// Exceptions are swallowed to avoid impacting the caller.
        /// </summary>
        public static void ScheduleBackground(Func<Task> work)
        {
            // Use 'ArgumentNullException.ThrowIfNull'
            ArgumentNullException.ThrowIfNull(work, nameof(work));

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Yield();
                    await work().ConfigureAwait(false);
                }
                catch
                {
                    // Intentionally swallow background errors; middleware may provide its own observability.
                }
            });
        }

        /// <summary>
        /// Performs a safe dispatch from background or async context. Prefers <see cref="IDispatchContext.DispatchAsync"/>
        /// when available and falls back to <see cref="IDispatchContext.Enqueue"/>. Returns a task that completes
        /// when the dispatched action has been enqueued or completed according to the dispatch mode.
        /// </summary>
        public static Task SafeDispatchAsync(IDispatchContext ctx, IAction action, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(ctx, nameof(ctx));
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            try
            {
                var vt = ctx.DispatchAsync(action, ct);
                return vt.IsCompletedSuccessfully ? Task.CompletedTask : vt.AsTask();
            }
            catch
            {
                // If DispatchAsync is not supported or throws, fall back to enqueueing.
                return ctx.Enqueue(action, ct);
            }
        }
    }
}
