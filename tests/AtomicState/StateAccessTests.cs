using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class StateAccessTests
    {
        [Fact]
        public void GetSliceState_WhenCombinedStateContainsSlice_ReturnsTypedSliceState()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var personSlice = TestSlices.CreatePersonSlice();
            var (rootState, _) = Factory.CombineReducers(counterSlice, personSlice);

            var counterState = StateAccess.GetSliceState(rootState, counterSlice);
            var personState = StateAccess.GetSliceState(rootState, personSlice);

            Assert.Equal(0, counterState.Count);
            Assert.Equal("Guest", personState.Name);
        }

        [Fact]
        public void GetSliceState_WhenRootStateIsNotCombined_ThrowsInvalidOperationException()
        {
            var counterSlice = TestSlices.CreateCounterSlice();

            var exception = Assert.Throws<InvalidOperationException>(() => StateAccess.GetSliceState(new CounterState(0), counterSlice));

            Assert.Contains(nameof(Factory.CombineReducers), exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void GetSliceState_WhenRootStateIsNull_ThrowsArgumentNullException()
        {
            var counterSlice = TestSlices.CreateCounterSlice();

            _ = Assert.Throws<ArgumentNullException>(() => StateAccess.GetSliceState(null!, counterSlice));
        }

        [Fact]
        public void GetSliceState_WhenSliceIsMissing_ThrowsKeyNotFoundException()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var personSlice = TestSlices.CreatePersonSlice();
            var (rootState, _) = Factory.CombineReducers(counterSlice);

            var exception = Assert.Throws<KeyNotFoundException>(() => StateAccess.GetSliceState(rootState, personSlice));

            Assert.Contains("person", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void GetSliceState_WhenSliceTypeDoesNotMatch_ThrowsInvalidCastException()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var mismatchedSlice = Factory.CreateSlice(
                name: "counter",
                initialState: new PersonState("Guest", 30),
                reducer: (state, action) => state);
            var (rootState, _) = Factory.CombineReducers(counterSlice);

            var exception = Assert.Throws<InvalidCastException>(() => StateAccess.GetSliceState(rootState, mismatchedSlice));

            Assert.Contains("counter", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void TryGetSliceState_WhenSliceExistsWithExpectedType_ReturnsTrueAndSliceState()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var (rootState, _) = Factory.CombineReducers(counterSlice);

            var found = StateAccess.TryGetSliceState(rootState, counterSlice, out var counterState);

            Assert.True(found);
            Assert.NotNull(counterState);
            Assert.Equal(0, counterState.Count);
        }

        [Fact]
        public void TryGetSliceState_WhenSliceDoesNotExist_ReturnsFalse()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var personSlice = TestSlices.CreatePersonSlice();
            var (rootState, _) = Factory.CombineReducers(counterSlice);

            var found = StateAccess.TryGetSliceState(rootState, personSlice, out var personState);

            Assert.False(found);
            Assert.Null(personState);
        }

        [Fact]
        public void TryGetSliceState_WhenSliceTypeDoesNotMatch_ReturnsFalse()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var mismatchedSlice = Factory.CreateSlice(
                name: "counter",
                initialState: new PersonState("Guest", 30),
                reducer: (state, action) => state);
            var (rootState, _) = Factory.CombineReducers(counterSlice);

            var found = StateAccess.TryGetSliceState(rootState, mismatchedSlice, out var personState);

            Assert.False(found);
            Assert.Null(personState);
        }
    }
}