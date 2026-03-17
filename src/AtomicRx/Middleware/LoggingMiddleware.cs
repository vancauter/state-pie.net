using SC.SimpleLog;

namespace AtomicRx
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
            SourceName = "AtomicRx",
            Template = "[{Timestamp}] <{SourceName}> {Message}"
        });

        /// <summary>
        /// Creates logging middleware.
        /// </summary>
        /// <returns>A middleware delegate that logs each dispatched action before forwarding it.</returns>
        /// <remarks>
        /// Logging failures are suppressed so diagnostics do not interrupt dispatch execution.
        /// </remarks>
        public static AtomicMiddleware Create() => (store, next) => action =>
        {
            try
            {
                s_log.Info($"Dispatched action: {action.Type}");
            }
            catch
            {
            }

            next(action);
        };
    }
}