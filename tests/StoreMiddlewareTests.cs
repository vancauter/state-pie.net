using Xunit;

namespace StatePie.Tests
{
    public class StoreMiddlewareTests
    {
        [Fact]
        public void GivenMiddlewareRegisteredInOrder_WhenDispatching_ThenEachWrapsTheNextStageInThatOrder()
        {
            var calls = new List<string>();
            var slice = CreateCounterSlice();
            void first(ICounterAction action, Action<ICounterAction> next)
            {
                calls.Add("first-before");
                next(action);
                calls.Add("first-after");
            }
            void second(ICounterAction action, Action<ICounterAction> next)
            {
                calls.Add("second-before");
                next(action);
                calls.Add("second-after");
            }

            using var store = StoreFactory.CreateStore([slice], first, second);
            using var subscription = store.Subscribe(_ => calls.Add("state-changed"));

            store.Dispatch(new IncrementCounterAction());

            Assert.Equal(
                ["first-before", "second-before", "state-changed", "second-after", "first-after"],
                calls);
            Assert.Equal(1, Assert.IsType<CounterState>(store.State["counter"]).Count);
        }

        [Fact]
        public void GivenMiddlewareDoesNotForward_WhenDispatching_ThenStateAndNotificationsRemainUnchanged()
        {
            var slice = CreateCounterSlice();
            static void middleware(ICounterAction _1, Action<ICounterAction> _2) { }
            using var store = StoreFactory.CreateStore([slice], middleware);
            var initialSnapshot = store.State;
            var notifications = 0;
            using var subscription = store.Subscribe(_ => notifications++);

            store.Dispatch(new IncrementCounterAction());

            Assert.Same(initialSnapshot, store.State);
            Assert.Equal(0, notifications);
        }

        [Fact]
        public void GivenMiddlewareForwardsMoreThanOnce_WhenDispatching_ThenSecondForwardIsRejected()
        {
            var slice = CreateCounterSlice();
            static void middleware(ICounterAction action, Action<ICounterAction> next)
            {
                next(action);
                next(action);
            }
            using var store = StoreFactory.CreateStore([slice], middleware);
            var notifications = 0;
            using var subscription = store.Subscribe(_ => notifications++);

            var exception = Assert.Throws<InvalidOperationException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Contains("at most once", exception.Message);
            Assert.Equal(1, Assert.IsType<CounterState>(store.State["counter"]).Count);
            Assert.Equal(1, notifications);
        }

        [Fact]
        public void GivenDisposedStore_WhenDispatching_ThenMiddlewareIsNotInvoked()
        {
            var slice = CreateCounterSlice();
            var middlewareCalls = 0;
            void middleware(ICounterAction action, Action<ICounterAction> next)
            {
                middlewareCalls++;
                next(action);
            }
            var store = StoreFactory.CreateStore([slice], middleware);
            store.Dispose();

            _ = Assert.Throws<ObjectDisposedException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Equal(0, middlewareCalls);
        }

        [Fact]
        public void GivenReducerThrows_WhenMiddlewareForwardsAction_ThenExceptionPropagatesWithoutRetry()
        {
            var expectedException = new InvalidOperationException("Reducer failed.");
            var reducerCalls = 0;
            var slice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: (_, _) =>
                {
                    reducerCalls++;
                    throw expectedException;
                });
            var nextCalls = 0;
            void middleware(ICounterAction action, Action<ICounterAction> next)
            {
                nextCalls++;
                next(action);
            }
            using var store = StoreFactory.CreateStore([slice], middleware);

            var actualException = Assert.Throws<InvalidOperationException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Same(expectedException, actualException);
            Assert.Equal(1, nextCalls);
            Assert.Equal(1, reducerCalls);
            Assert.Equal(0, Assert.IsType<CounterState>(store.State["counter"]).Count);
        }

        private static ISlice<ICounterAction> CreateCounterSlice() =>
            StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action switch
                {
                    IncrementCounterAction => state with { Count = state.Count + 1 },
                    _ => state
                });

        private sealed record CounterState(int Count) : IState;

        private interface ICounterAction : IAction;

        private sealed record IncrementCounterAction : ICounterAction;
    }
}
