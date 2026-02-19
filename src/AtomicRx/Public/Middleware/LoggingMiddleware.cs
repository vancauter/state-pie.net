using SCLibs.Logger;

namespace AtomicRx
{
    /// <summary>
    /// Simple logging middleware example that appends dispatched action types to the provided list
    /// and (optionally) forwards them to the shared `SCLibs.Logger` facade for immediate logging.
    /// </summary>
    public static class LoggingMiddleware
    {
        /// <summary>
        /// Create a logging middleware.
        /// </summary>
        /// <param name="sink">Optional in-memory sink that will receive action.Type strings.</param>
        /// <param name="useCoreLogger">If true, also write each action to <see cref="Log.Info(object)"/>.</param>
        public static Middleware Create(List<string>? sink, bool useCoreLogger = false) => (store, next) => action =>
        {
            try
            {
                // Append to the provided in-memory sink for test/demo purposes.
                sink?.Add(action.Type);

                // If requested, forward the action type immediately to the Logger library.
                if (useCoreLogger)
                {
                    // Keep logging calls minimal and non-throwing — logger internals
                    // guard against failures, but we still wrap to be defensive.
                    try { Log.Info(action.Type); } catch { }
                }
            }
            catch
            {
                // ignore logging errors
            }

            // Continue the pipeline; don't block dispatch with additional work.
            next(action);
        };
    }
}
