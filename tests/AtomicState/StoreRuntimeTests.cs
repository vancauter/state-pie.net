using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class StoreRuntimeTests
    {
        [Fact]
        public void CreateStore_WhenReducerHandlesInit_RecomputesStateImmediately()
        {
            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;
                return action.Type == "@@INIT"
                    ? new CounterState(currentState.Count + 5)
                    : state;
            }

            var slice = TestSlices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, rootReducer);

            Assert.Equal(5, ((CounterState)store.State).Count);
        }

        [Fact]
        public void CreateStore_WhenInitReducerThrows_CreatesUsableStore()
        {
            static IState rootReducer(IState state, IAction action)
            {
                if (action.Type == "@@INIT")
                {
                    throw new InvalidOperationException("init failure");
                }

                var currentState = (CounterState)state;
                return action.Type == TestSlices.Increment
                    ? new CounterState(currentState.Count + 1)
                    : state;
            }

            var slice = TestSlices.CreateCounterSlice();

            using var store = Factory.CreateStore(slice.State, rootReducer);

            Assert.Equal(0, ((CounterState)store.State).Count);

            store.Dispatch(Factory.CreateAction(TestSlices.Increment));

            Assert.Equal(1, ((CounterState)store.State).Count);
        }

        [Fact]
        public void State_WhenReadByObserverDuringNotification_MatchesPublishedState()
        {
            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;
                return action.Type == TestSlices.Increment
                    ? new CounterState(currentState.Count + 1)
                    : state;
            }

            var observedPairs = new List<(int Published, int Current)>();

            using var store = Factory.CreateStore(new CounterState(0), rootReducer);
            using var subscription = store.StateChanged.Subscribe(new CallbackObserver(state =>
            {
                observedPairs.Add((((CounterState)state).Count, ((CounterState)store.State).Count));
            }));

            observedPairs.Clear();

            store.Dispatch(Factory.CreateAction(TestSlices.Increment));
            store.Dispatch(Factory.CreateAction(TestSlices.Increment));
            store.Dispatch(Factory.CreateAction(TestSlices.Increment));

            Assert.Equal(
                [(1, 1), (2, 2), (3, 3)],
                observedPairs);
        }

        [Fact]
        public async Task State_WhenReadFromConcurrentReader_ObservesEachCommittedUpdate()
        {
            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;
                return action.Type == TestSlices.Increment
                    ? new CounterState(currentState.Count + 1)
                    : state;
            }

            const int dispatchCount = 256;

            using var store = Factory.CreateStore(new CounterState(0), rootReducer);
            using var stopReaders = new CancellationTokenSource();

            var committedTarget = 0;
            var observedTarget = 0;
            Exception? readerFailure = null;

            var reader = Task.Run(() =>
            {
                var lastObserved = ((CounterState)store.State).Count;

                while (!stopReaders.IsCancellationRequested)
                {
                    var current = ((CounterState)store.State).Count;
                    if (current < lastObserved)
                    {
                        readerFailure = new InvalidOperationException($"State moved backwards from {lastObserved} to {current}.");
                        return;
                    }

                    lastObserved = current;

                    var expected = Volatile.Read(ref committedTarget);
                    if (current >= expected)
                    {
                        Volatile.Write(ref observedTarget, current);
                    }

                    Thread.SpinWait(64);
                }
            }, stopReaders.Token);

            for (var expected = 1; expected <= dispatchCount; expected++)
            {
                store.Dispatch(Factory.CreateAction(TestSlices.Increment));
                Volatile.Write(ref committedTarget, expected);

                Assert.True(
                    SpinWait.SpinUntil(() => Volatile.Read(ref observedTarget) >= expected, TimeSpan.FromSeconds(1)),
                    $"Reader did not observe committed state {expected}.");
            }

            stopReaders.Cancel();
            await reader;

            Assert.Null(readerFailure);
            Assert.Equal(dispatchCount, ((CounterState)store.State).Count);
        }

        private sealed class CallbackObserver(Action<IState> onNext) : IObserver<IState>
        {
            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(IState value) => onNext(value);
        }
    }
}