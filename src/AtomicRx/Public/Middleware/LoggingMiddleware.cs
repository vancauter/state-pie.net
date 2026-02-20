using SCLibs.Logger;

namespace AtomicRx
{
    /// <summary>
    /// Simple logging middleware
    /// </summary>
    public static class LoggingMiddleware
    {
        // Static constructor to configure the logger when the middleware is first used.
        static LoggingMiddleware() =>
            // Configure the shared logger to write to console with a simple format.
            Log.SetOptions(new LogOptions
            {
                IncludeAppName = false,
                IncludeSeverity = false,
                IncludeTimestamp = true,
            });

        /// <summary>
        /// Create a logging middleware that sends action types to <see cref="Log.Info(object)"/>.
        /// </summary>
        public static Middleware Create() => (store, next) => action =>
        {
            try
            {
                Log.Info($"Dispatched action: {action.Type}");
            }
            catch
            {
                // swallow any unexpected logging exceptions — middleware must not
                // interfere with store behavior.
            }

            // Continue the pipeline.
            next(action);
        };
    }
}
