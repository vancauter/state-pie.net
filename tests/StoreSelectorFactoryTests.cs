using Xunit;

namespace StatePie.Tests
{
    public class StoreSelectorFactoryTests
    {
        [Fact]
        public void GivenSliceSelector_WhenKeyExistsAndTypeMatches_ThenTypedStateIsReturned()
        {
            var selector = StoreFactory.CreateSliceSelector<CounterState>("counter");
            IReadOnlyDictionary<string, IState> snapshot = new Dictionary<string, IState>
            {
                ["counter"] = new CounterState(7)
            };

            var state = selector(snapshot);

            Assert.Equal(7, state.Count);
        }

        [Fact]
        public void GivenSliceSelector_WhenSliceKeyIsMissing_ThenKeyNotFoundExceptionIsThrown()
        {
            var selector = StoreFactory.CreateSliceSelector<CounterState>("counter");
            IReadOnlyDictionary<string, IState> snapshot = new Dictionary<string, IState>
            {
                ["other"] = new CounterState(1)
            };

            _ = Assert.Throws<KeyNotFoundException>(() => selector(snapshot));
        }

        [Fact]
        public void GivenSliceSelector_WhenSliceStateTypeMismatches_ThenInvalidOperationExceptionIsThrown()
        {
            var selector = StoreFactory.CreateSliceSelector<CounterState>("counter");
            IReadOnlyDictionary<string, IState> snapshot = new Dictionary<string, IState>
            {
                ["counter"] = new UserState("alex")
            };

            _ = Assert.Throws<InvalidOperationException>(() => selector(snapshot));
        }

        [Fact]
        public void GivenSourceSelectorAndProjector_WhenCreatingSelector_ThenProjectedValueIsReturned()
        {
            var counterSelector = StoreFactory.CreateSliceSelector<CounterState>("counter");
            var countSelector = StoreFactory.CreateSelector(counterSelector, static state => state.Count);
            IReadOnlyDictionary<string, IState> snapshot = new Dictionary<string, IState>
            {
                ["counter"] = new CounterState(11)
            };

            var count = countSelector(snapshot);

            Assert.Equal(11, count);
        }

        [Fact]
        public void GivenNullProjector_WhenCreatingSelector_ThenArgumentNullExceptionIsThrown()
        {
            static CounterState source(IReadOnlyDictionary<string, IState> _) => new(1);
            _ = Assert.Throws<ArgumentNullException>(() => StoreFactory.CreateSelector<CounterState, int>(source, null!));
        }

        private sealed record CounterState(int Count) : IState;

        private sealed record UserState(string Name) : IState;
    }
}
