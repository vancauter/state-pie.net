using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class StoreMiddlewareDispatchTests
    {
        [Fact]
        public void Dispatch_WhenMiddlewareTriggersNestedDispatch_PreservesSerializedDispatchOrder()
        {
            const string outerAction = "counter.outer";
            const string nestedAction = "counter.nested";

            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;

                return action.Type switch
                {
                    outerAction => new CounterState(currentState.Count + 1),
                    nestedAction => new CounterState(currentState.Count + 10),
                    _ => state
                };
            }

            var timeline = new List<string>();

            Dispatch middleware(IStore store, Dispatch next) => action =>
            {
                timeline.Add($"before:{action.Type}");
                next(action);
                timeline.Add($"after:{action.Type}");

                if (action.Type == outerAction)
                {
                    // Re-enter after the outer pipeline unwinds so the timeline shows whether dispatches stay serialized.
                    timeline.Add("dispatch:nested");
                    store.Dispatch(Factory.CreateAction(nestedAction));
                    timeline.Add("after:nested");
                }
            };

            using var store = Factory.CreateStore(new CounterState(0), rootReducer, middleware);
            using var subscription = store.StateChanged.Subscribe(new CallbackObserver(state =>
            {
                timeline.Add($"state:{((CounterState)state).Count}");
            }));

            timeline.Clear();

            store.Dispatch(Factory.CreateAction(outerAction));

            Assert.Equal(
                [
                    $"before:{outerAction}",
                    "state:1",
                    $"after:{outerAction}",
                    "dispatch:nested",
                    $"before:{nestedAction}",
                    "state:11",
                    $"after:{nestedAction}",
                    "after:nested"
                ],
                timeline);
            Assert.Equal(11, ((CounterState)store.State).Count);
        }

        [Fact]
        public void Dispatch_WhenMiddlewareTriggersNestedDispatch_SerializesObserverCallbacks()
        {
            const string outerAction = "counter.outer";
            const string nestedAction = "counter.nested";

            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;

                return action.Type switch
                {
                    outerAction => new CounterState(currentState.Count + 1),
                    nestedAction => new CounterState(currentState.Count + 10),
                    _ => state
                };
            }

            var observedCounts = new List<int>();
            var activeCallbacks = 0;
            var maxConcurrentCallbacks = 0;

            Dispatch middleware(IStore store, Dispatch next) => action =>
            {
                next(action);

                if (action.Type == outerAction)
                {
                    store.Dispatch(Factory.CreateAction(nestedAction));
                }
            };

            using var store = Factory.CreateStore(new CounterState(0), rootReducer, middleware);
            using var subscription = store.StateChanged.Subscribe(new CallbackObserver(state =>
            {
                var currentConcurrency = Interlocked.Increment(ref activeCallbacks);
                UpdateMaximum(ref maxConcurrentCallbacks, currentConcurrency);
                observedCounts.Add(((CounterState)state).Count);

                // Hold the callback open long enough for a nested dispatch to overlap if observer notifications are not serialized.
                Thread.Sleep(10);

                _ = Interlocked.Decrement(ref activeCallbacks);
            }));

            observedCounts.Clear();

            store.Dispatch(Factory.CreateAction(outerAction));

            Assert.Equal([1, 11], observedCounts);
            Assert.Equal(1, maxConcurrentCallbacks);
        }

        [Fact]
        public async Task Dispatch_WhenMiddlewareSchedulesBackgroundWork_CompletesOriginalDispatchBeforeBackgroundDispatchSerializes()
        {
            const string outerAction = "counter.outer";
            const string backgroundAction = "counter.background";

            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;

                return action.Type switch
                {
                    outerAction => new CounterState(currentState.Count + 1),
                    backgroundAction => new CounterState(currentState.Count + 10),
                    _ => state
                };
            }

            var observedCounts = new List<int>();
            var backgroundStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var allowBackgroundDispatch = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var backgroundCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            Dispatch middleware(IStore store, Dispatch next) => action =>
            {
                if (action.Type == outerAction)
                {
                    _ = Task.Run(async () =>
                    {
                        _ = backgroundStarted.TrySetResult();

                        try
                        {
                            // This gate lets the test prove the original dispatch commits before background work re-enters.
                            await allowBackgroundDispatch.Task;
                            store.Dispatch(Factory.CreateAction(backgroundAction));
                            _ = backgroundCompleted.TrySetResult();
                        }
                        catch (Exception exception)
                        {
                            _ = backgroundCompleted.TrySetException(exception);
                        }
                    });
                }

                next(action);
            };

            using var store = Factory.CreateStore(new CounterState(0), rootReducer, middleware);
            using var subscription = store.StateChanged.Subscribe(new CallbackObserver(state =>
            {
                observedCounts.Add(((CounterState)state).Count);
            }));

            observedCounts.Clear();

            store.Dispatch(Factory.CreateAction(outerAction));

            await backgroundStarted.Task.WaitAsync(TimeSpan.FromSeconds(1));

            Assert.Equal(1, ((CounterState)store.State).Count);
            Assert.Equal([1], observedCounts);
            Assert.False(backgroundCompleted.Task.IsCompleted);

            allowBackgroundDispatch.SetResult();

            await backgroundCompleted.Task.WaitAsync(TimeSpan.FromSeconds(1));

            Assert.Equal(11, ((CounterState)store.State).Count);
            Assert.Equal([1, 11], observedCounts);
        }

        private static void UpdateMaximum(ref int target, int value)
        {
            while (true)
            {
                var snapshot = Volatile.Read(ref target);
                if (value <= snapshot)
                {
                    return;
                }

                if (Interlocked.CompareExchange(ref target, value, snapshot) == snapshot)
                {
                    return;
                }
            }
        }
    }
}