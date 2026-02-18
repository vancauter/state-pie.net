using SCLibs.StateContainer;

namespace Dev
{
    public class Program
    {
        private sealed class ConsoleObserver : IObserver<IState>
        {
            public void OnNext(IState state) => Console.WriteLine($"State changed: {Helpers.SerializeState(state)}");
            public void OnError(Exception error) => Console.Error.WriteLine($"Error: {error}");
            public void OnCompleted() => Console.WriteLine($"Completed.");
        }

        public sealed class CounterState(int count) : IState
        {
            public int Count { get; } = count;
        }

        public sealed class ParentState(string name, CounterState child) : IState
        {
            public CounterState Child { get; } = child;
            public string Name { get; } = name;
        }

        public static void Main()
        {
            var counterSlice = Factory.CreateSlice(
                name: "counter",
                initialState: new ParentState("root", new CounterState(0)),
                reducer: (state, action) =>
                {
                    return action.Type switch
                    {
                        "increment" => new ParentState(state.Name, new CounterState(state.Child.Count + 1)),
                        "decrement" => new ParentState(state.Name, new CounterState(state.Child.Count - 1)),
                        _ => state
                    };
                });

            var (state, reducer) = Factory.CombineReducers(counterSlice);

            using var store = Factory.CreateStore(state, reducer);

            var subscription = store.StateChanged.Subscribe(new ConsoleObserver());

            store.Dispatch(Factory.CreateAction("increment"));
            store.Dispatch(Factory.CreateAction("increment"));
            store.Dispatch(Factory.CreateAction("decrement"));

            subscription.Dispose();
        }
    }
}