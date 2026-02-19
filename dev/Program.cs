using AtomicRx;

namespace Dev
{
    public class Program
    {
        // Observer used by the dev harness. It accepts an optional completion callback
        // which allows the subscriber to clean up/dispose the subscription when the
        // store signals completion (OnCompleted).
        //
        // NOTE: the completion callback is invoked from the observer's OnCompleted
        // handler so the subscription itself can be disposed in response to the
        // store completing (see subscription creation below).
        private sealed class ConsoleObserver(Action? onCompleted = null) : IObserver<IState>
        {
            private readonly Action? _onCompleted = onCompleted;

            public void OnNext(IState state) => Console.WriteLine($"State changed: {Helpers.SerializeState(state)}");
            public void OnError(Exception error) => Console.Error.WriteLine($"Error: {error}");
            public void OnCompleted()
            {
                // Dispose callback invoked here to unsubscribe the observer when the
                // subject/store completes. This avoids holding references after completion.
                _onCompleted?.Invoke();
                Console.WriteLine("Completed.");
            }
        }

        public sealed record CounterState(int Count) : IState;

        public sealed record ParentState(string Name, CounterState Child) : IState;

        private const string Increment = "increment";
        private const string Decrement = "decrement";

        public static void Main()
        {
            var counterSlice = Factory.CreateSlice(
                name: "counter",
                initialState: new ParentState("root", new CounterState(0)),
                reducer: (state, action) =>
                {
                    return action.Type switch
                    {
                        Increment => new ParentState(state.Name, new CounterState(state.Child.Count + 1)),
                        Decrement => new ParentState(state.Name, new CounterState(state.Child.Count - 1)),
                        _ => state
                    };
                });

            var (state, reducer) = Factory.CombineReducers(counterSlice);

            using var store = Factory.CreateStore(state, reducer);

            // Create a local subscription variable and an observer that will dispose
            // the subscription when the store signals completion. This keeps the
            // subscription lifetime local to Main and avoids static state.
            IDisposable? subscription = null;
            var observer = new ConsoleObserver(() => subscription?.Dispose());
            subscription = store.StateChanged.Subscribe(observer);

            store.Dispatch(Factory.CreateAction(Increment));
            store.Dispatch(Factory.CreateAction(Increment));
            store.Dispatch(Factory.CreateAction(Decrement));

            // Disposing the store will call OnCompleted on the internal subject which
            // in turn invokes the observer's OnCompleted. The observer's completion
            // handler runs the provided callback, disposing the subscription.
            store.Dispose();
        }
    }
}