using SCLibs.Logger;

namespace AtomicRx
{
    /// <summary>
    /// Simple logging middleware example that forwards each dispatched action
    /// type immediately to the shared `SCLibs.Logger` facade.
    /// </summary>
    public static class LoggingMiddleware
    {
        /// <summary>
        /// Create a logging middleware that sends action types to <see cref="Log.Info(object)"/>.
        /// </summary>
        public static Middleware Create() => (store, next) => action =>
        {
            try
            {
                // Immediately hand off to the shared logger. Keep this call
                // defensive so logging cannot break dispatch.
                try { Log.Info(action.Type); } catch { }
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
