using System.Diagnostics;
using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class PerformanceTests
    {
        private const int s_dispatchIterations = 50_000;
        private const int s_warmupIterations = 2_000;

        [Trait("Category", "Perf")]
        [Fact]
        public void Dispatch_WhenReducerReturnsSameInstance_CompletesHighVolumeRunWithinReasonableTime()
        {
            static IState rootReducer(IState state, IAction action) => state;

            using var store = Factory.CreateStore(new CounterState(0), rootReducer);
            var action = Factory.CreateAction("perf.noop");

            Warmup(() => store.Dispatch(action));

            var elapsed = MeasureElapsed(() =>
            {
                for (var iteration = 0; iteration < s_dispatchIterations; iteration++)
                {
                    store.Dispatch(action);
                }
            });

            Assert.True(
                elapsed < TimeSpan.FromSeconds(2),
                $"Expected {s_dispatchIterations:N0} no-op dispatches to complete within 2 seconds, but took {elapsed.TotalMilliseconds:N0} ms.");
            Assert.Equal(0, ((CounterState)store.State).Count);
        }

        [Trait("Category", "Perf")]
        [Fact]
        public void Dispatch_WhenReducerReturnsSameInstance_KeepsAllocationsNearZero()
        {
            static IState rootReducer(IState state, IAction action) => state;

            using var store = Factory.CreateStore(new CounterState(0), rootReducer);
            var action = Factory.CreateAction("perf.noop");

            Warmup(() => store.Dispatch(action));

            var allocatedBytes = MeasureAllocatedBytes(() =>
            {
                for (var iteration = 0; iteration < s_dispatchIterations; iteration++)
                {
                    store.Dispatch(action);
                }
            });

            Assert.True(
                allocatedBytes <= 64 * 1024,
                $"Expected no-op dispatch path to stay close to allocation-free, but it allocated {allocatedBytes:N0} bytes.");
        }

        [Trait("Category", "Perf")]
        [Fact]
        public void Dispatch_WhenStateChangesAndObserverSubscribed_StaysWithinPerDispatchAllocationBudget()
        {
            static IState rootReducer(IState state, IAction action)
            {
                var currentState = (CounterState)state;

                return action.Type == TestSlices.Increment
                    ? new CounterState(currentState.Count + 1)
                    : state;
            }

            using var store = Factory.CreateStore(new CounterState(0), rootReducer);
            using var subscription = store.StateChanged.Subscribe(new NoOpObserver());
            var action = Factory.CreateAction(TestSlices.Increment);

            Warmup(() => store.Dispatch(action));

            var allocatedBytes = MeasureAllocatedBytes(() =>
            {
                for (var iteration = 0; iteration < s_dispatchIterations; iteration++)
                {
                    store.Dispatch(action);
                }
            });

            var bytesPerDispatch = allocatedBytes / (double)s_dispatchIterations;

            Assert.True(
                bytesPerDispatch <= 128,
                $"Expected changing dispatch path to stay within 128 bytes per dispatch on average, but it used {bytesPerDispatch:N2} bytes.");
            Assert.Equal(s_dispatchIterations + s_warmupIterations, ((CounterState)store.State).Count);
        }

        [Trait("Category", "Perf")]
        [Fact]
        public void Dispatch_WhenCombinedReducerHandlesNoOpAction_PreservesRootReferenceAndAvoidsPublicationGrowth()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var personSlice = TestSlices.CreatePersonSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(counterSlice, personSlice);

            using var store = Factory.CreateStore(rootState, rootReducer);
            using var observer = new CountingObserver();
            using var subscription = store.StateChanged.Subscribe(observer);
            var action = Factory.CreateAction("perf.combined.noop");
            var before = store.State;

            observer.Reset();
            Warmup(() => store.Dispatch(action));

            var allocatedBytes = MeasureAllocatedBytes(() =>
            {
                for (var iteration = 0; iteration < s_dispatchIterations; iteration++)
                {
                    store.Dispatch(action);
                }
            });

            Assert.Same(before, store.State);
            Assert.Equal(0, observer.NextCount);
            Assert.True(
                allocatedBytes <= 64 * 1024,
                $"Expected combined no-op dispatch path to stay close to allocation-free, but it allocated {allocatedBytes:N0} bytes.");
        }

        [Trait("Category", "Perf")]
        [Fact]
        public void Subscribe_WhenObserversAreDisposed_ReleasesObserverInstancesAfterCollection()
        {
            static IState rootReducer(IState state, IAction action) => state;

            const int observerCount = 2_000;

            using var store = Factory.CreateStore(new CounterState(0), rootReducer);
            var references = new List<WeakReference>(observerCount);

            for (var iteration = 0; iteration < observerCount; iteration++)
            {
                references.Add(CreateDisposedObserverReference(store));
            }

            ForceFullCollection();

            var survivors = references.Count(reference => reference.IsAlive);

            Assert.True(
                survivors <= 1,
                $"Expected disposed observers to be released after collection, but {survivors} references remained alive.");
        }

        private static WeakReference CreateDisposedObserverReference(IStore store)
        {
            var observer = new PayloadObserver();
            var weakReference = new WeakReference(observer);
            using var subscription = store.StateChanged.Subscribe(observer);

            return weakReference;
        }

        private static long MeasureAllocatedBytes(Action action)
        {
            ForceFullCollection();
            _ = GC.GetAllocatedBytesForCurrentThread();
            var before = GC.GetAllocatedBytesForCurrentThread();

            action();

            return GC.GetAllocatedBytesForCurrentThread() - before;
        }

        private static TimeSpan MeasureElapsed(Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        private static void Warmup(Action action)
        {
            for (var iteration = 0; iteration < s_warmupIterations; iteration++)
            {
                action();
            }
        }

        private static void ForceFullCollection()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private sealed class NoOpObserver : IObserver<IState>
        {
            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(IState value)
            {
            }
        }

        private sealed class CountingObserver : IObserver<IState>, IDisposable
        {
            public int NextCount { get; private set; }

            public void Reset() => NextCount = 0;

            public void Dispose()
            {
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(IState value) => NextCount++;
        }

        private sealed class PayloadObserver : IObserver<IState>
        {
            private readonly byte[] _payload = GC.AllocateUninitializedArray<byte>(1024);

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(IState value) => _ = _payload[0];
        }
    }
}