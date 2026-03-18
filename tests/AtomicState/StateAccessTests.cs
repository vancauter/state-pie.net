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

            var counterState = StateAccess.GetSliceState<CounterState>(rootState, "counter");
            var personState = StateAccess.GetSliceState<PersonState>(rootState, "person");

            Assert.Equal(0, counterState.Count);
            Assert.Equal("Guest", personState.Name);
        }

        [Fact]
        public void GetSliceState_WhenRootStateIsNotCombined_ThrowsInvalidOperationException()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => StateAccess.GetSliceState<CounterState>(new CounterState(0), "counter"));

            Assert.Contains(nameof(Factory.CombineReducers), exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void GetSliceState_WhenRootStateIsNull_ThrowsArgumentNullException() =>
         _ = Assert.Throws<ArgumentNullException>(() => StateAccess.GetSliceState<CounterState>(null!, "counter"));

        [Fact]
        public void GetSliceState_WhenSliceIsMissing_ThrowsKeyNotFoundException()
        {
            var (rootState, _) = Factory.CombineReducers(TestSlices.CreateCounterSlice());

            var exception = Assert.Throws<KeyNotFoundException>(() => StateAccess.GetSliceState<PersonState>(rootState, "person"));

            Assert.Contains("person", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void GetSliceState_WhenSliceTypeDoesNotMatch_ThrowsInvalidCastException()
        {
            var (rootState, _) = Factory.CombineReducers(TestSlices.CreateCounterSlice());

            var exception = Assert.Throws<InvalidCastException>(() => StateAccess.GetSliceState<PersonState>(rootState, "counter"));

            Assert.Contains("counter", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void TryGetSliceState_WhenSliceExistsWithExpectedType_ReturnsTrueAndSliceState()
        {
            var (rootState, _) = Factory.CombineReducers(TestSlices.CreateCounterSlice());

            var found = StateAccess.TryGetSliceState<CounterState>(rootState, "counter", out var counterState);

            Assert.True(found);
            Assert.NotNull(counterState);
            Assert.Equal(0, counterState.Count);
        }

        [Fact]
        public void TryGetSliceState_WhenSliceDoesNotExist_ReturnsFalse()
        {
            var (rootState, _) = Factory.CombineReducers(TestSlices.CreateCounterSlice());

            var found = StateAccess.TryGetSliceState<PersonState>(rootState, "person", out var personState);

            Assert.False(found);
            Assert.Null(personState);
        }

        [Fact]
        public void TryGetSliceState_WhenSliceTypeDoesNotMatch_ReturnsFalse()
        {
            var (rootState, _) = Factory.CombineReducers(TestSlices.CreateCounterSlice());

            var found = StateAccess.TryGetSliceState<PersonState>(rootState, "counter", out var personState);

            Assert.False(found);
            Assert.Null(personState);
        }
    }
}