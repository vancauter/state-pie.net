using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class MiddlewareTests
    {
        [Fact]
        public void LoggingMiddleware_WhenActionDispatched_RecordsActionTypeAndForwardsAction()
        {
            var slice = TestSlices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            var writer = new StringWriter();
            var originalOut = Console.Out;

            try
            {
                Console.SetOut(writer);

                using var store = Factory.CreateStore(rootState, rootReducer, LoggingMiddleware.Create());

                store.Dispatch(Factory.CreateAction(TestSlices.Increment));

                Console.SetOut(originalOut);

                var output = writer.ToString();
                var counterState = StoreStateAssertions.GetSliceState<CounterState>(store.State, "counter");

                Assert.Contains(TestSlices.Increment, output, StringComparison.Ordinal);
                Assert.Equal(1, counterState.Count);
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void LoggingMiddleware_WhenMultipleActionsDispatched_RecordsThemInDispatchOrder()
        {
            var slice = TestSlices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            var writer = new StringWriter();
            var originalOut = Console.Out;

            try
            {
                Console.SetOut(writer);

                using var store = Factory.CreateStore(rootState, rootReducer, LoggingMiddleware.Create());

                store.Dispatch(Factory.CreateAction(TestSlices.Increment));
                store.Dispatch(Factory.CreateAction(TestSlices.Increment));

                Console.SetOut(originalOut);

                var output = writer.ToString();
                var firstIncrementIndex = output.IndexOf(TestSlices.Increment, StringComparison.Ordinal);
                var secondIncrementIndex = output.IndexOf(TestSlices.Increment, firstIncrementIndex + 1, StringComparison.Ordinal);

                Assert.True(firstIncrementIndex >= 0, "Expected first increment to be logged");
                Assert.True(secondIncrementIndex > firstIncrementIndex, "Expected second increment after first increment");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
