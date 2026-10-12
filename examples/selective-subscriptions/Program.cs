namespace StatePie.Examples.SelectiveSubscriptions
{
    public interface IAppAction : IAction;

    public sealed record IncrementCounterAction : IAppAction;

    public sealed record SetCaptionAction(string Caption) : IAppAction;

    public sealed record CounterState(int Count) : IState;

    public sealed record CaptionState(string Value) : IState;

    public static class Program
    {
        public static void Main()
        {
            var counterSlice = StoreFactory.CreateSlice<CounterState, IAppAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action is IncrementCounterAction
                    ? state with { Count = state.Count + 1 }
                    : state);

            var captionSlice = StoreFactory.CreateSlice<CaptionState, IAppAction>(
                name: "caption",
                initialState: new CaptionState("Ready"),
                reducer: static (state, action) => action is SetCaptionAction set
                    ? state with { Value = set.Caption }
                    : state);

            using var store = StoreFactory.CreateStore(counterSlice, captionSlice);
            var selectCount = StoreFactory.CreateSelector(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                static state => state.Count);

            Console.WriteLine($"Initial count: {selectCount(store.State)}");

            using var countSubscription = store.Subscribe(
                selectCount,
                static count => Console.WriteLine($"Selected count changed: {count}"));

            // This updates another slice, so the count selector does not notify its listener.
            store.Dispatch(new SetCaptionAction("Working"));
            store.Dispatch(new IncrementCounterAction());
        }
    }
}
