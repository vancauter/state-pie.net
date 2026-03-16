using AtomicRx.Tests.TestSupport;

namespace AtomicRx.Tests
{
    public class StoreRuntimeTests
    {
        [Fact]
        public void HotSwapReducer_WhenReplaced_UsesNewLogic()
        {
            static IState oldRoot(IState state, IAction action) => state;

            static IState newRoot(IState state, IAction action)
            {
                var currentState = (CounterState)state;
                return action.Type == TestSlices.Increment
                    ? new CounterState(currentState.Count + 1)
                    : state;
            }

            var slice = TestSlices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, oldRoot);

            store.Dispatch(Factory.CreateAction(TestSlices.Increment));
            Assert.Equal(0, ((CounterState)store.State).Count);

            store.HotSwapReducer(newRoot);

            store.Dispatch(Factory.CreateAction(TestSlices.Increment));
            Assert.Equal(1, ((CounterState)store.State).Count);
        }

        [Fact]
        public void HotSwapReducer_WhenReducerHandlesInit_RecomputesState()
        {
            static IState oldRoot(IState state, IAction action) => state;

            static IState newRoot(IState state, IAction action)
            {
                var currentState = (CounterState)state;
                return action.Type == "@@INIT"
                    ? new CounterState(currentState.Count + 5)
                    : state;
            }

            var slice = TestSlices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, oldRoot);

            store.HotSwapReducer(newRoot);

            Assert.Equal(5, ((CounterState)store.State).Count);
        }
    }
}