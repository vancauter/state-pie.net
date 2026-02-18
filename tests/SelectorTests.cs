using AtomicRx;

namespace StateContainer.Tests
{
    public record AppState(int Value) : IState;

    public class SelectorTests
    {
        // Test: MemoizedSelectorCachesLastResultForSameInput
        // Why: Ensure the memoized selector caches the last computed result when the input
        //      is the same reference/value so expensive projections are not re-run.
        // What: Call the selector twice with the same `AppState` instance and assert the
        //       projection ran only once and produced the expected value.
        [Fact]
        public void MemoizedSelectorCachesLastResultForSameInput()
        {
            var callCount = 0;
            int projection(AppState s) { callCount++; return s.Value * 2; }

            var selector = Selectors.CreateMemoizedSelector((Func<AppState, int>)projection);

            var s = new AppState(10);
            var a = selector(s);
            var b = selector(s); // same reference

            Assert.Equal(20, a);
            Assert.Equal(20, b);
            Assert.Equal(1, callCount);
        }

        // Test: MemoizedSelectorRecomputesWhenComparerDiffers
        // Why: Verify caching semantics for record types: the default comparer uses structural
        //      equality, so two different instances with equal values should be treated as equal
        //      and avoid recomputation.
        // What: Call the selector with two separate `AppState` instances that are value-equal
        //       and assert the projection is only executed once due to caching behavior.
        [Fact]
        public void MemoizedSelectorRecomputesWhenComparerDiffers()
        {
            var callCount = 0;
            int projection(AppState s) { callCount++; return s.Value * 3; }

            var selector = Selectors.CreateMemoizedSelector((Func<AppState, int>)projection);

            var s1 = new AppState(2);
            var s2 = new AppState(2); // equal by value but different reference; default comparer will treat equal

            var a = selector(s1);
            var b = selector(s2);

            Assert.Equal(6, a);
            Assert.Equal(6, b);
            // Default comparer for record types uses structural equality so second call should be cached
            Assert.Equal(1, callCount);
        }
    }
}
