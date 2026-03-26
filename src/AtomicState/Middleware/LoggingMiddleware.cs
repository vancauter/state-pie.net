using Microsoft.Extensions.Logging;

namespace SC.AtomicState
{
    /// <summary>
    /// Provides middleware that logs dispatched action types through an <see cref="ILogger"/>.
    /// </summary>
    public static class LoggingMiddleware
    {
        private static readonly ILogger s_defaultLogger = new Logging.ConsoleLogger("AtomicState");

        private static readonly Action<ILogger, string, Exception?> s_logAction =
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(1, "ActionDispatched"), "Dispatched Action: {ActionType}");

        /// <summary>
        /// Creates middleware that logs each action type before forwarding the action.
        /// </summary>
        /// <param name="logger">Optional logger; when null a lightweight console logger is used.</param>
        /// <returns>A middleware delegate that logs each dispatched action before forwarding it.</returns>
        /// <remarks>
        /// The middleware logs before invoking the next dispatch delegate. Logging failures are suppressed so
        /// diagnostics do not interrupt dispatch execution.
        /// </remarks>
        public static Middleware Create(ILogger? logger = null) => (_, next) => action =>
        {
            try
            {
                // Use LoggerMessage delegate to avoid expensive formatting when logging is disabled.
                s_logAction(logger ?? s_defaultLogger, action.Type, null);
            }
            catch
            {
                // Diagnostics must never change dispatch behavior.
            }

            next(action);
        };


    }
}