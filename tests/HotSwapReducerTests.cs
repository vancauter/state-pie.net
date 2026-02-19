using AtomicRx.Tests.TestStore;

namespace AtomicRx.Tests
{
    public class ReducerReplaceTests
    {
        // Test HotSwapReducer Logic
        // Why: Ensure that swapping reducers at runtime changes the store's behavior
        //      so subsequent dispatches are interpreted by the new reducer.
        // What: Start with a no-op reducer, hot-swap to one that handles "INC",
        //       then dispatch and assert the state reflects the new reducer logic.
        [Fact]
        public void HotSwapReducerSwapsLogic()
        {
            // old reducer: no-op
            static IState oldRoot(IState state, IAction action) => state;

            // new reducer: handle increment by incrementing CounterState.Count
            static IState newRoot(IState state, IAction action)
            {
                var s = (CounterState)state;
                if (action.Type == Slices.Increment) return new CounterState(s.Count + 1);
                return state;
            }
            var slice = Slices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, oldRoot);

            // dispatch before swap should be a no-op
            store.Dispatch(Factory.CreateAction(Slices.Increment));
            Assert.Equal(0, ((CounterState)store.State).Count);

            // swap reducer
            store.HotSwapReducer(newRoot);

            // dispatch after swap should use new reducer logic
            store.Dispatch(Factory.CreateAction(Slices.Increment));
            Assert.Equal(1, ((CounterState)store.State).Count);
        }

        // Test HotSwapReducer Recomputed State
        // Why: Verify that the hot-swap performs an init pass allowing the new reducer
        //      to compute derived initial state immediately after being installed.
        // What: Hot-swap to a reducer that responds to the "@@INIT" action by
        //       adjusting state, and assert the store's state is updated.
        [Fact]
        public void HotSwapReducerInitPassPublishesRecomputedState()
        {
            // old reducer: no-op
            static IState oldRoot(IState state, IAction action) => state;

            // new reducer: on init action compute derived state (+5)
            static IState newRoot(IState state, IAction action)
            {
                var s = (CounterState)state;
                if (action.Type == "@@INIT") return new CounterState(s.Count + 5);
                return state;
            }
            var slice = Slices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, oldRoot);

            // swap reducer should run init pass and update the state
            store.HotSwapReducer(newRoot);

            Assert.Equal(5, ((CounterState)store.State).Count);
        }
    }
}
