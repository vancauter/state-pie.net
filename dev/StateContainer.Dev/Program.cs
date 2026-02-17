using System;
using SCLibs.StateContainer;

namespace Dev
{
    public class Program
    {
        private sealed class ConsoleObserver : IObserver<IState>
        {
            public void OnNext(IState state) => Console.WriteLine($"State changed: {state}");
            public void OnError(Exception error) { }
            public void OnCompleted() { }
        }

        public sealed record CounterState(int Count) : IState;

        public static void Main()
        {
            var counterSlice = Helpers.CreateSlice(
                name: "counter",
                initialState: new CounterState(0),
                reducerFn: (state, action) =>
                {
                    return action.Type switch
                    {
                        "increment" => new CounterState(state.Count + 1),
                        "decrement" => new CounterState(state.Count - 1),
                        _ => state
                    };
                });

            var (initialRootState, rootReducer) = Helpers.CombineReducers(counterSlice);

            using var store = new StateContainer(initialRootState, rootReducer);

            var subscription = store.StateChanged.Subscribe(new ConsoleObserver());

            store.Dispatch(Helpers.CreateAction("increment"));
            store.Dispatch(Helpers.CreateAction("increment"));
            store.Dispatch(Helpers.CreateAction("decrement"));

            subscription.Dispose();
        }
    }
}



