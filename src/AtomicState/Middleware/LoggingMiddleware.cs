using SC.SimpleLog;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides middleware that logs dispatched action types.
    /// </summary>
    public static class LoggingMiddleware
    {
        /// <summary>
        /// Stores the logger used by the middleware.
        /// </summary>
        private static readonly SimpleLogger s_log = new(new LogOptions()
        {
            SourceName = "AtomicState",
            Template = "[{Timestamp}] <{SourceName}> {Message}"
        });

        /// <summary>
        /// Creates logging middleware.
        /// </summary>
        /// <returns>A middleware delegate that logs each dispatched action before forwarding it.</returns>
        /// <remarks>
        /// Logging failures are suppressed so diagnostics do not interrupt dispatch execution.
        /// </remarks>
        public static Middleware Create() => (store, next) => action =>
        {
            try
            {
                s_log.Info($"Dispatched action: {action.Type}");
            }
            catch
            {
                // Diagnostics must never change dispatch behavior.
            }

            next(action);
        };
    }
}