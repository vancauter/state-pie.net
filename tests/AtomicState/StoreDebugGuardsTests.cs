using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SC.AtomicState.Tests
{
    public class StoreDebugGuardsTests
    {
        private sealed class TestState : IState { }

        [Fact]
        public async Task WarnOnLongDispatch_MiddlewareTriggersWarning()
        {
            var initial = new TestState();

            var tcs = new TaskCompletionSource<string?>();

            var options = new StoreOptions(DebugGuards.WarnOnLongDispatch, TimeSpan.FromMilliseconds(50), msg => tcs.TrySetResult(msg));

            // Middleware that blocks synchronously for 120ms before calling next
            Dispatch blocking(IDispatchContext ctx, Dispatch next) => action =>
            {
                Thread.Sleep(120);
                next(action);
            };

            var store = Factory.CreateStoreWithOptions(initial, (state, action) => state, options, blocking);

            store.Dispatch(Factory.CreateAction("TEST/NOOP"));

            var completed = await Task.WhenAny(tcs.Task, Task.Delay(1000));
            Assert.Equal(tcs.Task, completed);

            var warning = await tcs.Task;
            Assert.False(string.IsNullOrEmpty(warning));
            Assert.Contains("Long dispatch", warning);
        }

        [Fact]
        public async Task WarnOnLongDispatch_DefaultOff_NoWarning()
        {
            var initial = new TestState();
            var called = false;
            var options = new StoreOptions(DebugGuards.None, TimeSpan.FromMilliseconds(10), msg => called = true);

            Dispatch blocking(IDispatchContext ctx, Dispatch next) => action =>
            {
                Thread.Sleep(120);
                next(action);
            };

            var store = Factory.CreateStoreWithOptions(initial, (state, action) => state, options, blocking);
            store.Dispatch(Factory.CreateAction("TEST/NOOP"));

            await Task.Delay(200);

            Assert.False(called);
        }
    }
}
