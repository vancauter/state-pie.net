using Xunit;

namespace SnapshotStore.Tests
{
    public class StoreFactoryTests
    {
        [Fact]
        public void CreateStoreDispatchesStronglyTypedActionsAndUpdatesState()
        {
            // Why define a dedicated action family interface:
            // It gives the store a compile-time bounded action universe and prevents unrelated actions
            // from being dispatched accidentally.
            var counterSlice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action switch
                {
                    // Why pattern matching:
                    // Routing by runtime action type removes string keys and invalid payload pairings.
                    IncrementCounterAction => state with { Count = state.Count + 1 },
                    SetCounterAction set => state with { Count = set.Value },
                    _ => state
                });

            using var store = StoreFactory.CreateStore(counterSlice);

            var notifications = 0;
            store.OnStateChanged += _ => notifications++;

            store.Dispatch(new IncrementCounterAction());
            store.Dispatch(new SetCounterAction(10));

            var finalState = Assert.IsType<CounterState>(store.State["counter"]);
            Assert.Equal(10, finalState.Count);
            Assert.Equal(2, notifications);
        }

        [Fact]
        public void CreateStoreThrowsWhenSliceNamesAreDuplicated()
        {
            var first = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, _) => state);

            var second = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(100),
                reducer: static (state, _) => state);

            var exception = Assert.Throws<ArgumentException>(() => StoreFactory.CreateStore(first, second));
            Assert.Contains("Duplicate slice name", exception.Message);
        }

        // Why records:
        // Immutable action and state records make reducer transitions explicit and side-effect resistant.
        private sealed record CounterState(int Count) : IState;

        private interface ICounterAction : IAction;

        private sealed record IncrementCounterAction : ICounterAction;

        private sealed record SetCounterAction(int Value) : ICounterAction;
    }
}
