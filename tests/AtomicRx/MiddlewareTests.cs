using AtomicRx.Tests.TestSupport;

namespace AtomicRx.Tests
{
    public class MiddlewareTests
    {
        [Fact]
        public async Task ThunkMiddleware_WhenAsyncThunkDispatched_ExecutesPayload()
        {
            var slice = TestSlices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            using var store = Factory.CreateStore(rootState, rootReducer, ThunkMiddleware.Create());

            var thunk = new Func<IStore, Task>(async currentStore =>
            {
                currentStore.Dispatch(Factory.CreateAction(TestSlices.Increment));
                await Task.Delay(10);
                currentStore.Dispatch(Factory.CreateAction(TestSlices.Increment));
            });

            store.Dispatch(Factory.CreateAction("THUNK", thunk));

            await Task.Delay(50);

            var counterState = StoreStateAssertions.GetSliceState<CounterState>(store.State, "counter");
            Assert.Equal(2, counterState.Count);
        }

        [Fact]
        public void ThunkMiddleware_WhenSyncThunkDispatched_ExecutesPayload()
        {
            var slice = TestSlices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            using var store = Factory.CreateStore(rootState, rootReducer, ThunkMiddleware.Create());

            var thunk = new Action<IStore>(currentStore =>
            {
                currentStore.Dispatch(Factory.CreateAction(TestSlices.Increment));
                currentStore.Dispatch(Factory.CreateAction(TestSlices.Increment));
            });

            store.Dispatch(Factory.CreateAction("THUNK", thunk));

            var counterState = StoreStateAssertions.GetSliceState<CounterState>(store.State, "counter");
            Assert.Equal(2, counterState.Count);
        }

        [Fact]
        public async Task LoggingMiddleware_WhenThunkDispatchesNestedActions_RecordsThemInOrder()
        {
            var slice = TestSlices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            var writer = new StringWriter();
            var originalOut = Console.Out;

            try
            {
                Console.SetOut(writer);

                using var store = Factory.CreateStore(rootState, rootReducer, LoggingMiddleware.Create(), ThunkMiddleware.Create());

                var thunk = new Func<IStore, Task>(async currentStore =>
                {
                    currentStore.Dispatch(Factory.CreateAction(TestSlices.Increment));
                    await Task.Delay(5);
                    currentStore.Dispatch(Factory.CreateAction(TestSlices.Increment));
                });

                store.Dispatch(Factory.CreateAction("THUNK", thunk));
                await Task.Delay(50);

                Console.SetOut(originalOut);

                var output = writer.ToString();
                var thunkIndex = output.IndexOf("THUNK", StringComparison.Ordinal);
                var firstIncrementIndex = output.IndexOf(TestSlices.Increment, StringComparison.Ordinal);
                var secondIncrementIndex = output.IndexOf(TestSlices.Increment, firstIncrementIndex + 1, StringComparison.Ordinal);

                Assert.True(thunkIndex >= 0, "Expected THUNK to be logged");
                Assert.True(firstIncrementIndex > thunkIndex, "Expected first increment after THUNK");
                Assert.True(secondIncrementIndex > firstIncrementIndex, "Expected second increment after first increment");
            }
            finally
            {
                Console.SetOut(originalOut);
            }
        }
    }
}
