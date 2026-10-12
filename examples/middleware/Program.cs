namespace StatePie.Examples.Middleware
{
    public interface ICounterAction : IAction;

    public sealed record IncrementCounterAction : ICounterAction;

    public sealed record CounterState(int Count) : IState;

    public static class Program
    {
        public static void Main()
        {
            var counterSlice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action is IncrementCounterAction
                    ? state with { Count = state.Count + 1 }
                    : state);

            static void traceDispatch(ICounterAction action, Action<ICounterAction> next)
            {
                Console.WriteLine($"Before: {action.GetType().Name}");
                next(action);
                Console.WriteLine($"After: {action.GetType().Name}");
            }

            using var store = StoreFactory.CreateStore([counterSlice], traceDispatch);
            using var subscription = store.Subscribe(static snapshot =>
            {
                var counter = (CounterState)snapshot["counter"];
                Console.WriteLine($"Count: {counter.Count}");
            });

            store.Dispatch(new IncrementCounterAction());
        }
    }
}
