using AtomicRx;

namespace StateContainer.Tests
{
    public class ReducerReplaceTests
    {
        [Fact]
        public void HotSwapReducerSwapsLogic()
        {
            // old reducer: no-op
            Reducer oldRoot = (state, action) => state;

            // new reducer: handle "INC" by incrementing CounterState.Count
            Reducer newRoot = (state, action) =>
            {
                var s = (CounterState)state;
                if (action.Type == "INC") return new CounterState(s.Count + 1);
                return state;
            };

            using var store = Factory.CreateStore(new CounterState(0), oldRoot);

            // dispatch before swap should be a no-op
            store.Dispatch(Factory.CreateAction("INC"));
            Assert.Equal(0, ((CounterState)store.State).Count);

            // swap reducer
            store.HotSwapReducer(newRoot);

            // dispatch after swap should use new reducer logic
            store.Dispatch(Factory.CreateAction("INC"));
            Assert.Equal(1, ((CounterState)store.State).Count);
        }

        [Fact]
        public void HotSwapReducerInitPassPublishesRecomputedState()
        {
            // old reducer: no-op
            Reducer oldRoot = (state, action) => state;

            // new reducer: on init action compute derived state (+5)
            Reducer newRoot = (state, action) =>
            {
                var s = (CounterState)state;
                if (action.Type == "@@INIT") return new CounterState(s.Count + 5);
                return state;
            };

            using var store = Factory.CreateStore(new CounterState(0), oldRoot);

            // swap reducer should run init pass and update the state
            store.HotSwapReducer(newRoot);

            Assert.Equal(5, ((CounterState)store.State).Count);
        }
    }
}
