namespace AtomicRx
{
    /// <summary>
    /// Simple logging middleware example that appends dispatched action types to the provided list.
    /// </summary>
    public static class LoggingMiddleware
    {
        public static Middleware Create(List<string> sink) => (store, next) => action =>
        {
            try
            {
                sink?.Add(action.Type);
            }
            catch
            {
                // ignore logging errors
            }

            next(action);
        };
    }
}
