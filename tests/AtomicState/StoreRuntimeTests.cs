namespace SC.AtomicState.Tests
{
    public class StoreRuntimeTests
    {
        [Fact]
        public void CreateStore_WhenReducerHandlesInit_RecomputesStateImmediately()
        {
            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;
                return action.Type == "@@INIT"
                    ? new CounterState(currentState.Count + 5)
                    : state;
            }

            var slice = TestSlices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, rootReducer);

            Assert.Equal(5, ((CounterState)store.State).Count);
        }

        [Fact]
        public void CreateStore_WhenInitReducerThrows_CreatesUsableStore()
        {
            static IState rootReducer(IState state, IAction action)
            {
                if (action.Type == "@@INIT")
                {
                    throw new InvalidOperationException("init failure");
                }

                var currentState = (CounterState)state;
                return action.Type == TestSlices.Increment
                    ? new CounterState(currentState.Count + 1)
                    : state;
            }

            var slice = TestSlices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, rootReducer);

            Assert.Equal(0, ((CounterState)store.State).Count);

            store.Dispatch(Factory.CreateAction(TestSlices.Increment));

            Assert.Equal(1, ((CounterState)store.State).Count);
        }
    }
}