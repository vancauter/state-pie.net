using Xunit;

namespace StatePie.Tests
{
    public class StoreEffectTests
    {
        // Bounds awaits so a broken effect fails the test instead of hanging the run.
        private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(5);

        [Fact]
        public void GivenEffect_WhenActionIsDispatched_ThenEffectStartsAfterCommitAndNotification()
        {
            var calls = new List<string>();
            Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                calls.Add("effect");
                return Task.CompletedTask;
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);
            using var subscription = store.Subscribe(_ => calls.Add("state-changed"));

            store.Dispatch(new IncrementCounterAction());

            Assert.Equal(["state-changed", "effect"], calls);
        }

        [Fact]
        public void GivenEffectsRegisteredInOrder_WhenDispatching_ThenTheyStartInThatOrder()
        {
            var calls = new List<string>();
            Task first(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                calls.Add("first");
                return Task.CompletedTask;
            }
            Task second(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                calls.Add("second");
                return Task.CompletedTask;
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [first, second]);

            store.Dispatch(new IncrementCounterAction());

            Assert.Equal(["first", "second"], calls);
        }

        [Fact]
        public async Task GivenEffectDispatchesResult_WhenWorkCompletes_ThenResultIsReducedAndNotified()
        {
            var gate = CreateGate();
            var effectTasks = new List<Task>();
            Task effect(ICounterAction action, IEffectContext<ICounterAction> context)
            {
                if (action is not IncrementCounterAction)
                    return Task.CompletedTask;

                var task = RunAsync();
                effectTasks.Add(task);
                return task;

                async Task RunAsync()
                {
                    await gate.Task;
                    context.Dispatch(new SetCounterAction(7));
                }
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);
            var received = new List<int>();
            using var subscription = store.Subscribe(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                state => received.Add(state.Count));

            store.Dispatch(new IncrementCounterAction());

            Assert.Equal([1], received);

            gate.SetResult();
            await Assert.Single(effectTasks).WaitAsync(WaitLimit);

            Assert.Equal([1, 7], received);
        }

        [Fact]
        public async Task GivenConcurrentEffects_WhenTheyCompleteOutOfOrder_ThenResultsArriveInCompletionOrder()
        {
            var gates = new Dictionary<int, TaskCompletionSource>
            {
                [1] = CreateGate(),
                [2] = CreateGate()
            };
            var effectTasks = new List<Task>();
            Task effect(ICounterAction action, IEffectContext<ICounterAction> context)
            {
                if (action is not StartLoadAction start)
                    return Task.CompletedTask;

                var task = RunAsync(start.Id);
                effectTasks.Add(task);
                return task;

                async Task RunAsync(int id)
                {
                    await gates[id].Task;
                    context.Dispatch(new SetCounterAction(id));
                }
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);
            var received = new List<int>();
            using var subscription = store.Subscribe(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                state => received.Add(state.Count));
            store.Dispatch(new StartLoadAction(1));
            store.Dispatch(new StartLoadAction(2));

            gates[2].SetResult();
            await effectTasks[1].WaitAsync(WaitLimit);
            gates[1].SetResult();
            await effectTasks[0].WaitAsync(WaitLimit);

            Assert.Equal([2, 1], received);
        }

        [Fact]
        public void GivenEffectDispatchesBeforeItsFirstAwait_WhenDispatching_ThenResultIsCommittedImmediately()
        {
            Task effect(ICounterAction action, IEffectContext<ICounterAction> context)
            {
                if (action is IncrementCounterAction)
                    context.Dispatch(new SetCounterAction(10));

                return Task.CompletedTask;
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);
            var received = new List<int>();
            using var subscription = store.Subscribe(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                state => received.Add(state.Count));

            store.Dispatch(new IncrementCounterAction());

            Assert.Equal([1, 10], received);
        }

        [Fact]
        public void GivenMiddlewareDoesNotForward_WhenDispatching_ThenEffectsDoNotStart()
        {
            var effectCalls = 0;
            static void middleware(ICounterAction _1, Action<ICounterAction> _2) { }
            Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                effectCalls++;
                return Task.CompletedTask;
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [middleware], [effect]);

            store.Dispatch(new IncrementCounterAction());

            Assert.Equal(0, effectCalls);
        }

        [Fact]
        public void GivenReducerLeavesStateUnchanged_WhenDispatching_ThenEffectsStillStart()
        {
            var effectCalls = 0;
            Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                effectCalls++;
                return Task.CompletedTask;
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);
            var initialSnapshot = store.State;

            store.Dispatch(new IgnoredCounterAction());

            Assert.Same(initialSnapshot, store.State);
            Assert.Equal(1, effectCalls);
        }

        [Fact]
        public void GivenReducerThrows_WhenDispatching_ThenEffectsDoNotStart()
        {
            var effectCalls = 0;
            var slice = StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (_, _) => throw new InvalidOperationException("Reducer failed."));
            Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                effectCalls++;
                return Task.CompletedTask;
            }
            using var store = StoreFactory.CreateStore([slice], [], [effect]);

            _ = Assert.Throws<InvalidOperationException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Equal(0, effectCalls);
        }

        [Fact]
        public async Task GivenInFlightEffect_WhenStoreIsDisposed_ThenCancellationTokenIsCancelledAndNoErrorIsReported()
        {
            var errors = new List<Exception>();
            var effectTasks = new List<Task>();
            Task effect(ICounterAction _, IEffectContext<ICounterAction> context)
            {
                var task = Task.Delay(Timeout.Infinite, context.CancellationToken);
                effectTasks.Add(task);
                return task;
            }
            var store = StoreFactory.CreateStore(
                [CreateCounterSlice()],
                [],
                [effect],
                (exception, _) => errors.Add(exception));
            store.Dispatch(new IncrementCounterAction());
            var pending = Assert.Single(effectTasks);
            Assert.False(pending.IsCompleted);

            store.Dispose();

            _ = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
            Assert.Empty(errors);
        }

        [Fact]
        public async Task GivenDisposedStore_WhenEffectDispatchesLateResult_ThenResultIsDroppedWithoutFailing()
        {
            var gate = CreateGate();
            var effectTasks = new List<Task>();
            Task effect(ICounterAction action, IEffectContext<ICounterAction> context)
            {
                if (action is not IncrementCounterAction)
                    return Task.CompletedTask;

                var task = RunAsync();
                effectTasks.Add(task);
                return task;

                async Task RunAsync()
                {
                    await gate.Task;
                    context.Dispatch(new SetCounterAction(9));
                }
            }
            var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);
            var notifications = 0;
            using var subscription = store.Subscribe(_ => notifications++);
            store.Dispatch(new IncrementCounterAction());
            store.Dispose();

            gate.SetResult();
            await Assert.Single(effectTasks).WaitAsync(WaitLimit);

            Assert.Equal(1, notifications);
            Assert.Equal(1, Assert.IsType<CounterState>(store.State["counter"]).Count);
        }

        [Fact]
        public void GivenEffectThrowsSynchronously_WhenDispatching_ThenExceptionPropagatesAfterCommit()
        {
            var expectedException = new InvalidOperationException("Effect failed.");
            Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2) => throw expectedException;
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);

            var actualException = Assert.Throws<InvalidOperationException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Same(expectedException, actualException);
            Assert.Equal(1, Assert.IsType<CounterState>(store.State["counter"]).Count);
        }

        [Fact]
        public async Task GivenFaultedEffectTask_WhenErrorHandlerIsRegistered_ThenHandlerReceivesExceptionAndAction()
        {
            var expectedException = new InvalidOperationException("Effect failed.");
            var reported = new TaskCompletionSource<(Exception Exception, ICounterAction Action)>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2) => Task.FromException(expectedException);
            using var store = StoreFactory.CreateStore(
                [CreateCounterSlice()],
                [],
                [effect],
                (exception, action) => reported.TrySetResult((exception, action)));
            var dispatchedAction = new IncrementCounterAction();

            store.Dispatch(dispatchedAction);

            var (reportedException, reportedAction) = await reported.Task.WaitAsync(WaitLimit);
            Assert.Same(expectedException, reportedException);
            Assert.Same(dispatchedAction, reportedAction);
        }

        [Fact]
        public void GivenCancelledEffectTask_WhenErrorHandlerIsRegistered_ThenHandlerIsNotCalled()
        {
            var errors = new List<Exception>();
            static Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2) =>
                Task.FromCanceled(new CancellationToken(canceled: true));
            using var store = StoreFactory.CreateStore(
                [CreateCounterSlice()],
                [],
                [effect],
                (exception, _) => errors.Add(exception));

            store.Dispatch(new IncrementCounterAction());

            Assert.Empty(errors);
        }

        [Fact]
        public void GivenEffectReturnsNullTask_WhenDispatching_ThenInvalidOperationExceptionIsThrown()
        {
            static Task effect(ICounterAction _1, IEffectContext<ICounterAction> _2) => null!;
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);

            var exception = Assert.Throws<InvalidOperationException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Contains("must return a task", exception.Message);
        }

        [Fact]
        public void GivenNullActionFromEffect_WhenDispatchingResult_ThenArgumentNullExceptionIsThrown()
        {
            IEffectContext<ICounterAction>? capturedContext = null;
            Task effect(ICounterAction _, IEffectContext<ICounterAction> context)
            {
                capturedContext = context;
                return Task.CompletedTask;
            }
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], [effect]);
            store.Dispatch(new IncrementCounterAction());

            _ = Assert.Throws<ArgumentNullException>(() => capturedContext!.Dispatch(null!));
        }

        [Fact]
        public void GivenEffectsCollectionChangedAfterCreation_WhenDispatching_ThenOnlyOriginalEffectsRun()
        {
            var calls = new List<string>();
            Task first(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                calls.Add("first");
                return Task.CompletedTask;
            }
            Task second(ICounterAction _1, IEffectContext<ICounterAction> _2)
            {
                calls.Add("second");
                return Task.CompletedTask;
            }
            var effects = new List<Effect<ICounterAction>> { first };
            using var store = StoreFactory.CreateStore([CreateCounterSlice()], [], effects);
            effects.Add(second);

            store.Dispatch(new IncrementCounterAction());

            Assert.Equal(["first"], calls);
        }

        [Fact]
        public void GivenNullEffects_WhenCreatingStore_ThenArgumentNullExceptionIsThrown() =>
            _ = Assert.Throws<ArgumentNullException>(() =>
                StoreFactory.CreateStore([CreateCounterSlice()], [], (IReadOnlyList<Effect<ICounterAction>>)null!));

        [Fact]
        public void GivenNullEffectEntry_WhenCreatingStore_ThenArgumentExceptionIsThrown()
        {
            var exception = Assert.Throws<ArgumentException>(() =>
                StoreFactory.CreateStore([CreateCounterSlice()], [], [(Effect<ICounterAction>)null!]));

            Assert.Contains("null entries", exception.Message);
        }

        private static TaskCompletionSource CreateGate() => new(TaskCreationOptions.RunContinuationsAsynchronously);

        private static ISlice<ICounterAction> CreateCounterSlice() =>
            StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action switch
                {
                    IncrementCounterAction => state with { Count = state.Count + 1 },
                    SetCounterAction set => state with { Count = set.Value },
                    _ => state
                });

        private sealed record CounterState(int Count) : IState;

        private interface ICounterAction : IAction;

        private sealed record IncrementCounterAction : ICounterAction;

        private sealed record SetCounterAction(int Value) : ICounterAction;

        // Reducers ignore these, which exercises effects on no-op dispatches.
        private sealed record IgnoredCounterAction : ICounterAction;

        private sealed record StartLoadAction(int Id) : ICounterAction;
    }
}
