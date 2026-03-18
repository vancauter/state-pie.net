using SC.SimpleLog;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides middleware that logs dispatched action types through the shared AtomicState logger.
    /// </summary>
    public static class LoggingMiddleware
    {
        private static readonly SimpleLogger s_log = new(new LogOptions()
        {
            SourceName = "AtomicState",
            Template = "[{Timestamp}] <{SourceName}> {Message}"
        });

        /// <summary>
        /// Creates middleware that logs each action type before forwarding the action.
        /// </summary>
        /// <returns>A middleware delegate that logs each dispatched action before forwarding it.</returns>
        /// <remarks>
        /// The middleware uses one shared logger instance for all stores and logs before invoking the next dispatch
        /// delegate. Logging failures are suppressed so diagnostics do not interrupt dispatch execution.
        /// </remarks>
        public static Middleware Create() => (_, next) => action =>
        {
            try
            {
                s_log.Info($"Dispatched Action: {action.Type}");
            }
            catch
            {
                // Diagnostics must never change dispatch behavior.
            }

            next(action);
        };
    }
}