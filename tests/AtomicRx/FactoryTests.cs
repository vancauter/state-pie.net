namespace AtomicRx.Tests
{
    public record Counter(int Value) : IState;

    public class FactoryTests
    {
        // Test Create Action No Payload
        // Why: Ensure simple actions are created with the correct type and implement IAction.
        // What: Call Factory.CreateAction and verify the returned action's Type and interface.
        [Fact]
        public void CreateActionNoPayloadReturnsActionWithType()
        {
            var a = Factory.CreateAction("TEST_ACTION");
            Assert.Equal("TEST_ACTION", a.Type);
            _ = Assert.IsAssignableFrom<IAction>(a);
        }

        // Test Create Action With Payload
        // Why: Verify payload-carrying actions expose the payload and correct type.
        // What: Create an action with a payload and assert Type and Payload values.
        [Fact]
        public void CreateActionWithPayloadReturnsTypedAction()
        {
            var a = Factory.CreateAction("INC", 5);
            Assert.Equal("INC", a.Type);
            var ta = Assert.IsAssignableFrom<IAction<int>>(a);
            Assert.Equal(5, ta.Payload);
        }

        // Test Create Slice Basics
        // Why: Ensure slices expose name, initial state and a reducer that computes next state.
        // What: Create a slice and verify Name, State and reducer behavior for an action.
        [Fact]
        public void CreateSliceExposesNameStateAndReducer()
        {
            var slice = Factory.CreateSlice("counter", new Counter(0), (s, a) =>
            {
                return a.Type == "inc" ? new Counter(s.Value + 1) : s;
            });

            Assert.Equal("counter", slice.Name);
            _ = Assert.IsAssignableFrom<IState>(slice.State);

            var next = slice.Reducer(slice.State, Factory.CreateAction("inc"));
            Assert.NotSame(slice.State, next);
        }

        // Test Combine Reducers Integration
        // Why: Validate that combined reducers and the root store integrate and notify observers.
        // What: Combine two slices, create a store, subscribe, dispatch actions and assert observed updates.
        [Fact]
        public void CombineReducersAndCreateStoreIntegrateCorrectly()
        {
            var s1 = Factory.CreateSlice("a", new Counter(0), (s, a) => a.Type == "inc_a" ? new Counter(s.Value + 1) : s);
            var s2 = Factory.CreateSlice("b", new Counter(10), (s, a) => a.Type == "inc_b" ? new Counter(s.Value + 2) : s);

            var (rootState, rootReducer) = Factory.CombineReducers(s1, s2);

            using var store = Factory.CreateStore(rootState, rootReducer);

            var observed = new List<IState>();
            var collector = new TestObserver(observed);
            using var sub = store.StateChanged.Subscribe(collector);

            // initial state emitted immediately
            Assert.NotEmpty(observed);

            store.Dispatch(Factory.CreateAction("inc_a"));
            store.Dispatch(Factory.CreateAction("inc_b"));

            // After two updates we should have at least three observed states (initial + 2 updates)
            Assert.True(observed.Count >= 3);

            // Basic sanity: serialized state contains both slice names
            var ser = Helpers.SerializeState(store.State);
            Assert.Contains("\"a\":", ser);
            Assert.Contains("\"b\":", ser);
        }

        private sealed class TestObserver(List<IState> dest) : IObserver<IState>
        {
            private readonly List<IState> _dest = dest;

            public void OnCompleted() { }
            public void OnError(Exception error) { }
            public void OnNext(IState value) => _dest.Add(value);
        }
    }
}
