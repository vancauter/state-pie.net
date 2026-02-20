using AtomicRx;
using SCLibs.Logger;

namespace Dev
{
    public class Program
    {
        public static void Main()
        {
            // Create the example store with logging middleware applied.
            using var store = ExampleStore.Store.Create(LoggingMiddleware.Create());

            IDisposable? subscription = null;
            var observer = new ConsoleObserver(() => subscription?.Dispose());
            subscription = store.StateChanged.Subscribe(observer);

            // Counter slice
            store.Dispatch(Factory.CreateAction(ExampleStore.Slices.Increment));

            // Person slice
            store.Dispatch(Factory.CreateAction(ExampleStore.Slices.PersonAddAge));
            store.Dispatch(Factory.CreateAction(ExampleStore.Slices.PersonSetName, "Lee-Lee"));

            // Disposing the store will call OnCompleted on the internal subject which
            // in turn invokes the observer's OnCompleted. The observer's completion
            // handler runs the provided callback, disposing the subscription.
            store.Dispose();
        }

        // Simple console observer that prints state changes and errors.
        private sealed class ConsoleObserver(Action? unsubscribe = null) : IObserver<IState>
        {
            private readonly Action? _unsubscribe = unsubscribe;

            public void OnNext(IState state) => Log.Debug($"State changed: {Helpers.SerializeState(state)}");
            public void OnError(Exception error) { _unsubscribe?.Invoke(); Log.Error($"Error: {error}"); }
            public void OnCompleted() { _unsubscribe?.Invoke(); Log.Info("Unsubscribed and completed."); }
        }
    }
}
