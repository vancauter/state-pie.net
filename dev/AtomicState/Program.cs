using System.Text.Json;
using SC.SimpleLog;

namespace SC.AtomicState.Dev
{
    public class Program
    {
        private static readonly SimpleLogger s_log = new();

        public static void Main()
        {
            // Create the example store with logging middleware applied.
            using var store = ExampleStore.Create(LoggingMiddleware.Create());

            IDisposable? subscription = null;
            var observer = new ConsoleObserver(() => subscription?.Dispose());
            subscription = store.StateChanged.Subscribe(observer);

            // Counter slice
            store.Dispatch(Factory.CreateAction(ExampleStore.Increment));

            // Person slice
            store.Dispatch(Factory.CreateAction(ExampleStore.PersonAddAge));
            store.Dispatch(Factory.CreateAction(ExampleStore.PersonSetName, "Lee-Lee"));

            // Disposing the store will call OnCompleted on the internal subject which
            // in turn invokes the observer's OnCompleted. The observer's completion
            // handler runs the provided callback, disposing the subscription.
            store.Dispose();
        }

        // Simple console observer that prints state changes and errors.
        private sealed class ConsoleObserver(Action? unsubscribe = null) : IObserver<IState>
        {
            private readonly Action? _unsubscribe = unsubscribe;

            public void OnNext(IState state) => s_log.Debug($"State changed: {JsonSerializer.Serialize(state)}");
            public void OnError(Exception error) { _unsubscribe?.Invoke(); s_log.Error($"Error: {error}"); }
            public void OnCompleted() { _unsubscribe?.Invoke(); s_log.Info("Unsubscribed and completed."); }
        }
    }
}
