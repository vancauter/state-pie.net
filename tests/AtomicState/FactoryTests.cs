using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class FactoryTests
    {
        private static void AssertInvalidArgument(Action action, string parameterName, string messageFragment)
        {
            var exception = Assert.Throws<ArgumentException>(action);

            Assert.Equal(parameterName, exception.ParamName);
            Assert.Contains(messageFragment, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CreateAction_WithoutPayload_ReturnsTypedAction()
        {
            var action = Factory.CreateAction("TEST_ACTION");
            Assert.Equal("TEST_ACTION", action.Type);
            _ = Assert.IsAssignableFrom<IAction>(action);
        }

        [Fact]
        public void CreateAction_WithPayload_ReturnsTypedAction()
        {
            var action = Factory.CreateAction("INC", 5);
            Assert.Equal("INC", action.Type);
            var typedAction = Assert.IsAssignableFrom<IAction<int>>(action);
            Assert.Equal(5, typedAction.Payload);
        }

        [Fact]
        public void CreateAction_WhenTypeIsNull_ThrowsArgumentException() => AssertInvalidArgument(() => Factory.CreateAction(null!), "type", "Action type must be a non-empty string.");

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void CreateAction_WhenTypeIsBlank_ThrowsArgumentException(string type) => AssertInvalidArgument(() => Factory.CreateAction(type), "type", "Action type must be a non-empty string.");

        [Fact]
        public void CreateAction_WithPayload_WhenTypeIsNull_ThrowsArgumentException() => AssertInvalidArgument(() => Factory.CreateAction(null!, "payload"), "type", "Action type must be a non-empty string.");

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void CreateAction_WithPayload_WhenTypeIsBlank_ThrowsArgumentException(string type) => AssertInvalidArgument(() => Factory.CreateAction(type, 1), "type", "Action type must be a non-empty string.");

        [Fact]
        public void CreateSlice_WhenCreated_ExposesNameStateAndReducer()
        {
            var slice = Factory.CreateSlice("counter", new CounterState(0), (state, action) =>
            {
                return action.Type == "inc" ? new CounterState(state.Count + 1) : state;
            });

            Assert.Equal("counter", slice.Name);
            _ = Assert.IsAssignableFrom<IState>(slice.State);

            var next = slice.Reducer(slice.State, Factory.CreateAction("inc"));
            Assert.NotSame(slice.State, next);
        }

        [Fact]
        public void CreateSlice_WhenNameIsNull_ThrowsArgumentException() => AssertInvalidArgument(() => Factory.CreateSlice(null!, new CounterState(0), (state, action) => state), "name", "Slice name must be a non-empty string.");

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void CreateSlice_WhenNameIsBlank_ThrowsArgumentException(string name) => AssertInvalidArgument(() => Factory.CreateSlice(name, new CounterState(0), (state, action) => state), "name", "Slice name must be a non-empty string.");

        [Fact]
        public void CombineReducers_WhenStoreDispatches_UpdatesCombinedSliceState()
        {
            var firstSlice = Factory.CreateSlice("a", new CounterState(0), (state, action) => action.Type == "inc_a" ? new CounterState(state.Count + 1) : state);
            var secondSlice = Factory.CreateSlice("b", new CounterState(10), (state, action) => action.Type == "inc_b" ? new CounterState(state.Count + 2) : state);

            var (rootState, rootReducer) = Factory.CombineReducers(firstSlice, secondSlice);

            using var store = Factory.CreateStore(rootState, rootReducer);

            var observed = new List<IState>();
            var collector = new ListObserver(observed);
            using var subscription = store.StateChanged.Subscribe(collector);

            Assert.NotEmpty(observed);

            store.Dispatch(Factory.CreateAction("inc_a"));
            store.Dispatch(Factory.CreateAction("inc_b"));

            Assert.True(observed.Count >= 3);

            var firstState = StateAccess.GetSliceState<CounterState>(store.State, "a");
            var secondState = StateAccess.GetSliceState<CounterState>(store.State, "b");

            Assert.Equal(1, firstState.Count);
            Assert.Equal(12, secondState.Count);
        }

        [Fact]
        public void CombineReducers_WhenNoSlicesProvided_ThrowsArgumentException() => _ = Assert.Throws<ArgumentException>(() => Factory.CombineReducers());

        [Fact]
        public void CombineReducers_WhenSliceCollectionContainsNull_ThrowsArgumentException() => _ = Assert.Throws<ArgumentException>(() => Factory.CombineReducers(TestSlices.CreateCounterSlice(), null!));

        [Fact]
        public void CombineReducers_WhenSliceNamesDuplicate_ThrowsArgumentException()
        {
            var firstSlice = Factory.CreateSlice("counter", new CounterState(0), (state, action) => state);
            var secondSlice = Factory.CreateSlice("counter", new CounterState(1), (state, action) => state);

            _ = Assert.Throws<ArgumentException>(() => Factory.CombineReducers(firstSlice, secondSlice));
        }
    }
}
