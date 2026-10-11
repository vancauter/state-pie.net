using SnapshotStore;

namespace SnapshotStore.Examples.OneSlice;

// One action family per store keeps dispatch strongly typed.
public interface ICounterAction : IAction;

public sealed record IncrementCounterAction : ICounterAction;

public sealed record SetCounterAction(int Value) : ICounterAction;

public sealed record CounterState(int Count) : IState;

public static class Program
{
    public static void Main()
    {
        var counterSlice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
            name: "counter",
            initialState: new CounterState(0),
            reducer: static (state, action) => action switch
            {
                IncrementCounterAction => state with { Count = state.Count + 1 },
                SetCounterAction set => state with { Count = set.Value },
                _ => state
            });

        using var store = StoreFactory.CreateStore<ICounterAction>(counterSlice);

        store.OnStateChanged += static snapshot =>
        {
            var counter = (CounterState)snapshot["counter"];
            Console.WriteLine($"Count: {counter.Count}");
        };

        store.Dispatch(new IncrementCounterAction());
        store.Dispatch(new SetCounterAction(10));
    }
}
