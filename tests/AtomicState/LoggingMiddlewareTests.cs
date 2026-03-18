using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    [CollectionDefinition(CollectionName, DisableParallelization = true)]
    public sealed class ConsoleOutputIsolation
    {
        public const string CollectionName = "ConsoleOutput";
    }

    [Collection(ConsoleOutputIsolation.CollectionName)]
    public class LoggingMiddlewareTests
    {
        private static string CaptureConsoleOutput(Action action)
        {
            using var writer = new StringWriter();
            var originalOut = Console.Out;

            try
            {
                Console.SetOut(writer);
                action();
                return writer.ToString();
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void LoggingMiddleware_WhenActionDispatched_RecordsActionTypeAndForwardsAction()
        {
            var slice = TestSlices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);
            CounterState? counterState = null;

            var output = CaptureConsoleOutput(() =>
            {
                // The middleware writes to the process-wide console, so the test captures output explicitly and
                // relies on the non-parallel collection to avoid cross-test interference.
                using var store = Factory.CreateStore(rootState, rootReducer, LoggingMiddleware.Create());

                store.Dispatch(Factory.CreateAction(TestSlices.Increment));
                counterState = StateAccess.GetSliceState<CounterState>(store.State, "counter");
            });

            Assert.Contains(TestSlices.Increment, output, StringComparison.Ordinal);
            Assert.NotNull(counterState);
            Assert.Equal(1, counterState.Count);
        }

        [Fact]
        public void LoggingMiddleware_WhenMultipleActionsDispatched_RecordsThemInDispatchOrder()
        {
            var slice = TestSlices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            var output = CaptureConsoleOutput(() =>
            {
                // Capture the global console writer so the assertions can verify relative log ordering from one dispatch sequence.
                using var store = Factory.CreateStore(rootState, rootReducer, LoggingMiddleware.Create());

                store.Dispatch(Factory.CreateAction(TestSlices.Increment));
                store.Dispatch(Factory.CreateAction(TestSlices.Increment));
            });

            var firstIncrementIndex = output.IndexOf(TestSlices.Increment, StringComparison.Ordinal);
            var secondIncrementIndex = output.IndexOf(TestSlices.Increment, firstIncrementIndex + 1, StringComparison.Ordinal);

            Assert.True(firstIncrementIndex >= 0, "Expected first increment to be logged");
            Assert.True(secondIncrementIndex > firstIncrementIndex, "Expected second increment after first increment");
        }
    }
}
