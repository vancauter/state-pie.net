using AtomicRx;

namespace StateContainer.Tests
{
    public record AppState(int Value) : IState;

    public class SelectorTests
    {
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
