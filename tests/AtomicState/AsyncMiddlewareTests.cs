using System.Diagnostics;
using SC.AtomicState.Helpers;

namespace SC.AtomicState.Tests
{
    public class AsyncMiddlewareTests
    {
        private sealed record TestState(int Value) : IState;

        [Fact]
        public async Task UnsafeMiddleware_BlocksDispatch()
        {
            Dispatch badMw(IDispatchContext ctx, Dispatch next) => action =>
            {
                if (action.Type == "DO")
                {
                    // Simulate an await inside dispatch by blocking the thread.
                    Thread.Sleep(200);
                    ctx.Dispatch(Factory.CreateAction("DONE"));
                }

                next(action);
            };

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "DO") return new TestState(((TestState)state).Value + 1);
                if (action.Type == "DONE") return new TestState(((TestState)state).Value + 10);
                return state;
            });

            var store = Factory.CreateStore(new TestState(0), reducer, badMw);

            var sw = Stopwatch.StartNew();
            store.Dispatch(Factory.CreateAction("DO"));
            sw.Stop();

            // Bad middleware blocks and performs nested dispatch synchronously.
            Assert.Equal(11, ((TestState)store.State).Value);
            Assert.True(sw.ElapsedMilliseconds >= 150, "Dispatch should have been blocked by the middleware.");
        }

        [Fact]
        public async Task SafeMiddleware_DoesNotBlockDispatch()
        {
            Dispatch goodMw(IDispatchContext ctx, Dispatch next) => action =>
            {
                if (action.Type == "DO")
                {
                    MiddlewareHelpers.ScheduleBackground(async () =>
                    {
                        await Task.Delay(200);
                        await MiddlewareHelpers.SafeDispatchAsync(ctx, Factory.CreateAction("DONE"));
                    });
                }

                next(action);
            };

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "DO") return new TestState(((TestState)state).Value + 1);
                if (action.Type == "DONE") return new TestState(((TestState)state).Value + 10);
                return state;
            });

            var store = Factory.CreateStore(new TestState(0), reducer, goodMw);

            var sw = Stopwatch.StartNew();
            store.Dispatch(Factory.CreateAction("DO"));
            sw.Stop();

            // Immediate state update from the DO action only.
            Assert.Equal(1, ((TestState)store.State).Value);
            Assert.True(sw.ElapsedMilliseconds < 100, "Dispatch should not be blocked by background work.");

            // Background dispatch should eventually apply.
            TestState? final = null;
            var timeout = Stopwatch.StartNew();
            while (timeout.Elapsed < TimeSpan.FromSeconds(3))
            {
                if (((TestState)store.State).Value == 11)
                {
                    final = (TestState)store.State;
                    break;
                }

                await Task.Delay(10);
            }

            Assert.NotNull(final);
            Assert.Equal(11, final!.Value);
        }
    }
}
