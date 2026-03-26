using SC.AtomicState.Telemetry;

namespace SC.AtomicState.Tests
{
    public class MetricsTests
    {
        private sealed record TestState(int Value) : IState;

        private sealed class TestMetrics : IStoreMetrics
        {
            public int DispatchStartCount;
            public int DispatchEndCount;
            public int MiddlewareStartCount;
            public int MiddlewareEndCount;
            public TimeSpan LastDispatchElapsed;

            public void OnDispatchStart() => DispatchStartCount++;
            public void OnDispatchEnd(TimeSpan elapsed) { DispatchEndCount++; LastDispatchElapsed = elapsed; }
            public void OnMiddlewareStart(string middlewareId) => MiddlewareStartCount++;
            public void OnMiddlewareEnd(string middlewareId, TimeSpan elapsed) => MiddlewareEndCount++;
        }

        [Fact]
        public void Metrics_AreInvoked_ForDispatchAndMiddleware()
        {
            var metrics = new TestMetrics();

            Dispatch mw1(IDispatchContext ctx, Dispatch next) => action => next(action);
            Dispatch mw2(IDispatchContext ctx, Dispatch next) => action => next(action);

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "INC") return new TestState(((TestState)state).Value + 1);
                return state;
            });

            var store = Factory.CreateStore(new TestState(0), reducer, metrics, mw1, mw2);

            var beforeDispatchStart = metrics.DispatchStartCount;
            var beforeDispatchEnd = metrics.DispatchEndCount;
            var beforeMiddlewareStart = metrics.MiddlewareStartCount;
            var beforeMiddlewareEnd = metrics.MiddlewareEndCount;

            store.Dispatch(Factory.CreateAction("INC"));

            Assert.Equal(beforeDispatchStart + 1, metrics.DispatchStartCount);
            Assert.Equal(beforeDispatchEnd + 1, metrics.DispatchEndCount);
            // Each dispatch runs through both middleware entries.
            Assert.Equal(beforeMiddlewareStart + 2, metrics.MiddlewareStartCount);
            Assert.Equal(beforeMiddlewareEnd + 2, metrics.MiddlewareEndCount);
            Assert.True(metrics.LastDispatchElapsed >= TimeSpan.Zero);
        }
    }
}
