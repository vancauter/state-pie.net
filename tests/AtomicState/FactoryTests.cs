using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public record Counter(int Value) : IState;

    public class FactoryTests
    {
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
        public void CreateAction_WhenTypeIsNull_ThrowsArgumentException() => _ = Assert.Throws<ArgumentException>(() => Factory.CreateAction(null!));

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void CreateAction_WhenTypeIsBlank_ThrowsArgumentException(string type) => _ = Assert.Throws<ArgumentException>(() => Factory.CreateAction(type));

        [Fact]
        public void CreateActionWithPayload_WhenTypeIsNull_ThrowsArgumentException() => _ = Assert.Throws<ArgumentException>(() => Factory.CreateAction(null!, "payload"));

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void CreateActionWithPayload_WhenTypeIsBlank_ThrowsArgumentException(string type) => _ = Assert.Throws<ArgumentException>(() => Factory.CreateAction(type, 1));

        [Fact]
        public void CreateSlice_WhenCreated_ExposesNameStateAndReducer()
        {
            var slice = Factory.CreateSlice("counter", new Counter(0), (state, action) =>
            {
                return action.Type == "inc" ? new Counter(state.Value + 1) : state;
            });

            Assert.Equal("counter", slice.Name);
            _ = Assert.IsAssignableFrom<IState>(slice.State);

            var next = slice.Reducer(slice.State, Factory.CreateAction("inc"));
            Assert.NotSame(slice.State, next);
        }

        [Fact]
        public void CreateSlice_WhenNameIsNull_ThrowsArgumentException() => _ = Assert.Throws<ArgumentException>(() => Factory.CreateSlice(null!, new Counter(0), (state, action) => state));

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void CreateSlice_WhenNameIsBlank_ThrowsArgumentException(string name) => _ = Assert.Throws<ArgumentException>(() => Factory.CreateSlice(name, new Counter(0), (state, action) => state));

        [Fact]
        public void CombineReducers_WhenStoreDispatches_EmitsUpdatedRootState()
        {
            var firstSlice = Factory.CreateSlice("a", new Counter(0), (state, action) => action.Type == "inc_a" ? new Counter(state.Value + 1) : state);
            var secondSlice = Factory.CreateSlice("b", new Counter(10), (state, action) => action.Type == "inc_b" ? new Counter(state.Value + 2) : state);

            var (rootState, rootReducer) = Factory.CombineReducers(firstSlice, secondSlice);

            using var store = Factory.CreateStore(rootState, rootReducer);

            var observed = new List<IState>();
            var collector = new TestObserver(observed);
            using var subscription = store.StateChanged.Subscribe(collector);

            Assert.NotEmpty(observed);

            store.Dispatch(Factory.CreateAction("inc_a"));
            store.Dispatch(Factory.CreateAction("inc_b"));

            Assert.True(observed.Count >= 3);

            var firstState = StoreStateAssertions.GetSliceState<Counter>(store.State, "a");
            var secondState = StoreStateAssertions.GetSliceState<Counter>(store.State, "b");

            Assert.Equal(1, firstState.Value);
            Assert.Equal(12, secondState.Value);
        }

        [Fact]
        public void CombineReducers_WhenNoSlicesProvided_ThrowsArgumentException() => _ = Assert.Throws<ArgumentException>(() => Factory.CombineReducers());

        [Fact]
        public void CombineReducers_WhenSliceCollectionContainsNull_ThrowsArgumentException() => _ = Assert.Throws<ArgumentException>(() => Factory.CombineReducers(TestSlices.CreateCounterSlice(), null!));

        [Fact]
        public void CombineReducers_WhenSliceNamesDuplicate_ThrowsArgumentException()
        {
            var firstSlice = Factory.CreateSlice("counter", new Counter(0), (state, action) => state);
            var secondSlice = Factory.CreateSlice("counter", new Counter(1), (state, action) => state);

            _ = Assert.Throws<ArgumentException>(() => Factory.CombineReducers(firstSlice, secondSlice));
        }

        private sealed class TestObserver(List<IState> destination) : IObserver<IState>
        {
            private readonly List<IState> _destination = destination;

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(IState value) => _destination.Add(value);
        }
    }
}
