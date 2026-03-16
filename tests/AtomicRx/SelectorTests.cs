namespace AtomicRx.Tests
{
    public record AppState(int Value) : IState;

    public class SelectorTests
    {
        [Fact]
        public void MemoizedSelector_WhenInputMatchesCache_ReusesPreviousResult()
        {
            var callCount = 0;
            int projection(AppState state)
            {
                callCount++;
                return state.Value * 2;
            }

            var selector = Selectors.CreateMemoizedSelector((Func<AppState, int>)projection);

            var state = new AppState(10);
            var first = selector(state);
            var second = selector(state);

            Assert.Equal(20, first);
            Assert.Equal(20, second);
            Assert.Equal(1, callCount);
        }

        [Fact]
        public void MemoizedSelector_WhenComparerTreatsValuesEqual_ReusesPreviousResult()
        {
            var callCount = 0;
            int projection(AppState state)
            {
                callCount++;
                return state.Value * 3;
            }

            var selector = Selectors.CreateMemoizedSelector((Func<AppState, int>)projection);

            var firstState = new AppState(2);
            var secondState = new AppState(2);

            var first = selector(firstState);
            var second = selector(secondState);

            Assert.Equal(6, first);
            Assert.Equal(6, second);
            Assert.Equal(1, callCount);
        }
    }
}
