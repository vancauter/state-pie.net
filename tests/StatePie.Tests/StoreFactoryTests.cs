using Xunit;

namespace StatePie.Tests
{
    public class StoreFactoryTests
    {
        [Fact]
        public void GivenStoreWithOneSlice_WhenDispatchingMatchingActions_ThenStateIsUpdatedAndNotificationsAreRaised()
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

            using var store = StoreFactory.CreateStore(counterSlice);

            var notifications = 0;
            using var subscription = store.Subscribe(_ => notifications++);

            store.Dispatch(new IncrementCounterAction());
            store.Dispatch(new SetCounterAction(10));

            var finalState = Assert.IsType<CounterState>(store.State["counter"]);
            Assert.Equal(10, finalState.Count);
            Assert.Equal(2, notifications);
        }

        [Fact]
        public void GivenTwoSlicesWithSameName_WhenCreatingStore_ThenArgumentExceptionIsThrown()
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

        [Fact]
        public void GivenNullSliceArray_WhenCreatingStore_ThenArgumentExceptionIsThrown()
        {
            var exception = Assert.Throws<ArgumentException>(() => StoreFactory.CreateStore((ISlice<ICounterAction>[])null!));
            Assert.Contains("at least one slice", exception.Message);
        }

        [Fact]
        public void GivenStoreWithNoOpReducer_WhenDispatchingAction_ThenSnapshotReferenceAndNotificationCountStayUnchanged()
        {
            var slice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(5),
                reducer: static (state, _) => state);

            using var store = StoreFactory.CreateStore(slice);

            var before = store.State;
            var notifications = 0;
            using var subscription = store.Subscribe(_ => notifications++);

            store.Dispatch(new IncrementCounterAction());

            Assert.Same(before, store.State);
            Assert.Equal(0, notifications);
        }

        [Fact]
        public void GivenDisposedStore_WhenDispatchingAction_ThenObjectDisposedExceptionIsThrown()
        {
            var slice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, _) => state);

            using var store = StoreFactory.CreateStore(slice);
            store.Dispose();

            _ = Assert.Throws<ObjectDisposedException>(() => store.Dispatch(new IncrementCounterAction()));
        }

        [Fact]
        public void GivenEmptySliceName_WhenCreatingSlice_ThenArgumentExceptionIsThrown()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                StoreFactory.CreateSlice<CounterState, ICounterAction>(
                    name: "  ",
                    initialState: new CounterState(0),
                    reducer: static (state, _) => state));

            Assert.Contains("Slice name", exception.Message);
        }

        [Fact]
        public void GivenNullReducer_WhenCreatingSlice_ThenArgumentNullExceptionIsThrown() =>
            _ = Assert.Throws<ArgumentNullException>(() =>
                StoreFactory.CreateSlice<CounterState, ICounterAction>(
                    name: "counter",
                    initialState: new CounterState(0),
                    reducer: null!));

        private sealed record CounterState(int Count) : IState;

        private interface ICounterAction : IAction;

        private sealed record IncrementCounterAction : ICounterAction;

        private sealed record SetCounterAction(int Value) : ICounterAction;
    }
}
