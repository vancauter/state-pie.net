using Microsoft.Extensions.Logging;

namespace SC.AtomicState.Logging
{
    /// <summary>
    /// Lightweight console-backed <see cref="ILogger"/> used as a fallback when no logger is provided.
    /// </summary>
    public sealed class ConsoleLogger : ILogger
    {
        private readonly string _name;

        public ConsoleLogger(string name) => _name = name;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                var message = formatter(state, exception);
                Console.WriteLine($"[{DateTime.UtcNow:O}] <{_name}> {message}");
            }
            catch
            {
                // Swallow logging errors for diagnostic helpers.
            }
        }
    }
}
