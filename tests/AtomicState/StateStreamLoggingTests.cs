using Microsoft.Extensions.Logging;
using SC.AtomicState.Internal;
using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class StateStreamLoggingTests
    {
        private sealed class TestLogger : ILogger
        {
            public sealed record LogEntry(LogLevel Level, EventId EventId, string Message, Exception? Exception);

            public List<LogEntry> Entries { get; } = [];

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter(state, exception);
                Entries.Add(new LogEntry(logLevel, eventId, message, exception));
            }

            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();
                public void Dispose() { }
            }
        }

        [Fact]
        public void OnNext_WhenObserverThrows_LogsWarning()
        {
            var logger = new TestLogger();

            using var stream = new StateStream(new CounterState(0), logger);
            using var subscription = stream.Subscribe(new ThrowingObserver(throwOnNext: true));

            stream.OnNext(new CounterState(1));

            Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Exception is InvalidOperationException);
        }

        [Fact]
        public void OnError_WhenObserverThrows_LogsWarning()
        {
            var logger = new TestLogger();

            using var stream = new StateStream(new CounterState(0), logger);
            using var subscription = stream.Subscribe(new ThrowingObserver(throwOnError: true));

            stream.OnError(new InvalidOperationException("boom"));

            Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Exception is InvalidOperationException);
        }

        [Fact]
        public void OnCompleted_WhenObserverThrows_LogsWarning()
        {
            var logger = new TestLogger();

            using var stream = new StateStream(new CounterState(0), logger);
            using var subscription = stream.Subscribe(new ThrowingObserver(throwOnCompleted: true));

            stream.OnCompleted();

            Assert.Contains(logger.Entries, e => e.Level == LogLevel.Warning && e.Exception is InvalidOperationException);
        }
    }
}
