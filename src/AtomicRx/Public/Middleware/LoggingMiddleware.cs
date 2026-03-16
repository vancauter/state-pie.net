using SC.SimpleLog;

namespace AtomicRx
{
    /// <summary>
    /// Simple logging middleware
    /// </summary>
    public static class LoggingMiddleware
    {
        private static readonly SimpleLogger s_log = new();

        public static Middleware Create() => (store, next) => action =>
        {
            try
            {
                s_log.Info($"Dispatched action: {action.Type}");
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
