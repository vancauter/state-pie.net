using SC.AtomicState.Internal;
using SC.AtomicState.Tests.Shared;

namespace SC.AtomicState.Tests
{
    public class ReplayStateStreamTests
    {
        [Fact]
        public void Subscribe_WhenSubscriberJoinsAfterStateUpdates_ReplaysLatestState()
        {
            using var stream = new StateStream(new CounterState(0));

            stream.OnNext(new CounterState(1));
            stream.OnNext(new CounterState(2));

            var observer = new RecordingObserver();
            using var subscription = stream.Subscribe(observer);

            var replayedState = Assert.IsType<CounterState>(Assert.Single(observer.States));
            Assert.Equal(2, replayedState.Count);
            Assert.Equal(0, observer.CompletedCount);
            Assert.Null(observer.Error);
        }

        [Fact]
        public void OnCompleted_WhenStreamCompletes_ReplaysLastStateAndCompletionToFutureSubscribers()
        {
            using var stream = new StateStream(new CounterState(0));

            var currentObserver = new RecordingObserver();
            using var currentSubscription = stream.Subscribe(currentObserver);
            currentObserver.Clear();

            stream.OnNext(new CounterState(3));
            stream.OnCompleted();

            Assert.Equal(1, currentObserver.CompletedCount);
            Assert.Null(currentObserver.Error);
            Assert.Equal(3, Assert.IsType<CounterState>(Assert.Single(currentObserver.States)).Count);

            var futureObserver = new RecordingObserver();
            using var futureSubscription = stream.Subscribe(futureObserver);

            Assert.Equal(3, Assert.IsType<CounterState>(Assert.Single(futureObserver.States)).Count);
            Assert.Equal(1, futureObserver.CompletedCount);
            Assert.Null(futureObserver.Error);
        }

        [Fact]
        public void OnCompleted_WhenStreamAlreadyCompleted_IgnoresRepeatedCompletion()
        {
            using var stream = new StateStream(new CounterState(0));

            var observer = new RecordingObserver();
            using var subscription = stream.Subscribe(observer);
            observer.Clear();

            stream.OnCompleted();
            observer.Clear();

            stream.OnCompleted();

            var futureObserver = new RecordingObserver();
            using var futureSubscription = stream.Subscribe(futureObserver);

            Assert.Empty(observer.States);
            Assert.Equal(0, observer.CompletedCount);
            Assert.Null(observer.Error);
            Assert.Equal(1, futureObserver.CompletedCount);
            Assert.Null(futureObserver.Error);
        }

        [Fact]
        public void OnError_WhenStreamFaults_ReplaysLastStateAndErrorToFutureSubscribers()
        {
            using var stream = new StateStream(new CounterState(0));

            var currentObserver = new RecordingObserver();
            using var currentSubscription = stream.Subscribe(currentObserver);
            currentObserver.Clear();

            var failure = new InvalidOperationException("boom");

            stream.OnNext(new CounterState(4));
            stream.OnError(failure);

            Assert.Same(failure, currentObserver.Error);
            Assert.Equal(0, currentObserver.CompletedCount);
            Assert.Equal(4, Assert.IsType<CounterState>(Assert.Single(currentObserver.States)).Count);

            var futureObserver = new RecordingObserver();
            using var futureSubscription = stream.Subscribe(futureObserver);

            Assert.Equal(4, Assert.IsType<CounterState>(Assert.Single(futureObserver.States)).Count);
            Assert.Same(failure, futureObserver.Error);
            Assert.Equal(0, futureObserver.CompletedCount);
        }

        [Fact]
        public void OnError_WhenStreamAlreadyTerminated_IgnoresSubsequentErrors()
        {
            using var stream = new StateStream(new CounterState(0));

            var observer = new RecordingObserver();
            using var subscription = stream.Subscribe(observer);
            observer.Clear();

            var firstFailure = new InvalidOperationException("first");

            stream.OnError(firstFailure);
            observer.Clear();

            stream.OnError(new InvalidOperationException("second"));

            var futureObserver = new RecordingObserver();
            using var futureSubscription = stream.Subscribe(futureObserver);

            Assert.Empty(observer.States);
            Assert.Equal(0, observer.CompletedCount);
            Assert.Same(firstFailure, futureObserver.Error);
            Assert.Equal(0, futureObserver.CompletedCount);
        }

        [Fact]
        public void Dispose_WhenObserverSubscriptionDisposed_StopsFutureStateNotifications()
        {
            using var stream = new StateStream(new CounterState(0));

            var observer = new RecordingObserver();
            using var subscription = stream.Subscribe(observer);
            observer.Clear();

            subscription.Dispose();
            stream.OnNext(new CounterState(1));

            Assert.Empty(observer.States);
            Assert.Equal(0, observer.CompletedCount);
            Assert.Null(observer.Error);
        }

        [Fact]
        public void Subscribe_WhenSameObserverSubscribedTwice_DeliversFutureUpdatesOnlyOnce()
        {
            using var stream = new StateStream(new CounterState(0));

            var observer = new RecordingObserver();
            using var firstSubscription = stream.Subscribe(observer);
            using var secondSubscription = stream.Subscribe(observer);
            observer.Clear();

            stream.OnNext(new CounterState(5));

            Assert.Equal(5, Assert.IsType<CounterState>(Assert.Single(observer.States)).Count);
        }

        [Fact]
        public void Subscribe_WhenStreamDisposed_ThrowsObjectDisposedException()
        {
            using var stream = new StateStream(new CounterState(0));
            stream.Dispose();

            _ = Assert.Throws<ObjectDisposedException>(() => stream.Subscribe(new RecordingObserver()));
        }

        [Fact]
        public void OnNext_WhenStreamIsTerminated_IgnoresAdditionalStateUpdates()
        {
            using var completedStream = new StateStream(new CounterState(0));
            var completedObserver = new RecordingObserver();
            using var completedSubscription = completedStream.Subscribe(completedObserver);
            completedObserver.Clear();
            completedStream.OnCompleted();
            completedObserver.Clear();
            completedStream.OnNext(new CounterState(1));

            Assert.Empty(completedObserver.States);

            using var faultedStream = new StateStream(new CounterState(0));
            var faultedObserver = new RecordingObserver();
            using var faultedSubscription = faultedStream.Subscribe(faultedObserver);
            faultedObserver.Clear();
            faultedStream.OnError(new InvalidOperationException("boom"));
            faultedObserver.Clear();
            faultedStream.OnNext(new CounterState(1));

            Assert.Empty(faultedObserver.States);

            using var disposedStream = new StateStream(new CounterState(0));
            var disposedObserver = new RecordingObserver();
            using var disposedSubscription = disposedStream.Subscribe(disposedObserver);
            disposedObserver.Clear();
            disposedStream.Dispose();
            disposedObserver.Clear();
            disposedStream.OnNext(new CounterState(1));

            Assert.Empty(disposedObserver.States);
        }

        [Fact]
        public void Notifications_WhenObserverThrows_SwallowsObserverExceptions()
        {
            using var errorStream = new StateStream(new CounterState(0));
            using var throwingNextAndErrorSubscription = errorStream.Subscribe(new ThrowingObserver(throwOnNext: true, throwOnError: true));

            errorStream.OnNext(new CounterState(1));
            errorStream.OnError(new InvalidOperationException("boom"));

            using var completedStream = new StateStream(new CounterState(0));
            using var throwingCompletedSubscription = completedStream.Subscribe(new ThrowingObserver(throwOnCompleted: true));

            completedStream.OnCompleted();
        }

        [Fact]
        public void Dispose_WhenStreamDisposed_CompletesCurrentObserversAndIgnoresRepeatedDispose()
        {
            using var stream = new StateStream(new CounterState(0));

            var observer = new RecordingObserver();
            using var subscription = stream.Subscribe(observer);
            observer.Clear();

            stream.Dispose();
            stream.Dispose();

            Assert.Equal(1, observer.CompletedCount);
            Assert.Null(observer.Error);
            Assert.Empty(observer.States);
        }

        [Fact]
        public async Task Subscribe_WhenRacedWithOnNext_ReplaysPreviousStateOrCurrentStateWithoutDuplication()
        {
            const int iterationCount = 128;

            for (var iteration = 1; iteration <= iterationCount; iteration++)
            {
                using var stream = new StateStream(new CounterState(0));
                using var barrier = new Barrier(2);

                var observer = new ConcurrentRecordingObserver();
                var subscribeTask = StartConcurrentSubscribe(stream, observer, barrier);
                var publishTask = StartConcurrentAction(barrier, () => stream.OnNext(new CounterState(iteration)));

                using var subscription = await subscribeTask;
                await publishTask;

                var counts = observer.GetCounts();

                Assert.True(
                    // Depending on which side wins the race, the subscriber may observe either the published state
                    // directly or the initial replay followed by the newly published state, but never duplicates.
                    counts is [var current] && current == iteration ||
                    counts is [0, var replayedCurrent] && replayedCurrent == iteration,
                    $"Unexpected replay sequence: [{string.Join(", ", counts)}].");
                Assert.Equal(0, observer.CompletedCount);
                Assert.Null(observer.Error);
            }
        }

        [Fact]
        public async Task Subscribe_WhenRacedWithOnCompleted_ReplaysLatestStateAndCompletesOnce()
        {
            const int iterationCount = 128;

            for (var iteration = 0; iteration < iterationCount; iteration++)
            {
                using var stream = new StateStream(new CounterState(0));
                using var barrier = new Barrier(2);

                var observer = new ConcurrentRecordingObserver();
                var subscribeTask = StartConcurrentSubscribe(stream, observer, barrier);
                var completeTask = StartConcurrentAction(barrier, stream.OnCompleted);

                using var subscription = await subscribeTask;
                await completeTask;

                Assert.Equal([0], observer.GetCounts());
                Assert.Equal(1, observer.CompletedCount);
                Assert.Null(observer.Error);
            }
        }

        [Fact]
        public async Task Subscribe_WhenRacedWithOnError_ReplaysLatestStateAndPropagatesOneTerminalError()
        {
            const int iterationCount = 128;

            for (var iteration = 0; iteration < iterationCount; iteration++)
            {
                using var stream = new StateStream(new CounterState(0));
                using var barrier = new Barrier(2);

                var observer = new ConcurrentRecordingObserver();
                var failure = new InvalidOperationException($"boom-{iteration}");
                var subscribeTask = StartConcurrentSubscribe(stream, observer, barrier);
                var errorTask = StartConcurrentAction(barrier, () => stream.OnError(failure));

                using var subscription = await subscribeTask;
                await errorTask;

                Assert.Equal([0], observer.GetCounts());
                Assert.Equal(0, observer.CompletedCount);
                Assert.Same(failure, observer.Error);
            }
        }

        [Fact]
        public async Task Subscribe_WhenRacedWithDispose_CompletesObserverOrThrowsObjectDisposedException()
        {
            const int iterationCount = 128;

            for (var iteration = 0; iteration < iterationCount; iteration++)
            {
                using var stream = new StateStream(new CounterState(0));
                using var barrier = new Barrier(2);

                var observer = new ConcurrentRecordingObserver();
                var subscribeTask = StartConcurrentSubscribe(stream, observer, barrier);
                var disposeTask = StartConcurrentAction(barrier, stream.Dispose);

                IDisposable? subscription = null;
                ObjectDisposedException? disposedException = null;

                try
                {
                    subscription = await subscribeTask;
                }
                catch (ObjectDisposedException exception)
                {
                    disposedException = exception;
                }

                await disposeTask;

                if (disposedException is not null)
                {
                    Assert.Empty(observer.GetCounts());
                    Assert.Equal(0, observer.CompletedCount);
                    Assert.Null(observer.Error);
                    continue;
                }

                using (subscription!)
                {
                }

                Assert.Equal([0], observer.GetCounts());
                Assert.Equal(1, observer.CompletedCount);
                Assert.Null(observer.Error);
            }
        }

        private static Task<IDisposable> StartConcurrentSubscribe(StateStream stream, IObserver<IState> observer, Barrier barrier) =>
            Task.Run(() =>
            {
                // The barrier forces subscription and the competing stream action to start from the same point so the
                // test exercises real ordering races instead of deterministic sequencing.
                barrier.SignalAndWait();
                return stream.Subscribe(observer);
            });

        private static Task StartConcurrentAction(Barrier barrier, Action action) =>
            Task.Run(() =>
            {
                barrier.SignalAndWait();
                action();
            });

        private sealed class RecordingObserver : IObserver<IState>
        {
            public List<IState> States { get; } = [];

            public int CompletedCount { get; private set; }

            public Exception? Error { get; private set; }

            public void Clear()
            {
                States.Clear();
                CompletedCount = 0;
                Error = null;
            }

            public void OnCompleted() => CompletedCount++;

            public void OnError(Exception error) => Error = error;

            public void OnNext(IState value) => States.Add(value);
        }

        private sealed class ThrowingObserver(bool throwOnNext = false, bool throwOnError = false, bool throwOnCompleted = false) : IObserver<IState>
        {
            public void OnCompleted()
            {
                if (throwOnCompleted)
                {
                    throw new InvalidOperationException("completed");
                }
            }

            public void OnError(Exception error)
            {
                if (throwOnError)
                {
                    throw new InvalidOperationException("error");
                }
            }

            public void OnNext(IState value)
            {
                if (throwOnNext)
                {
                    throw new InvalidOperationException("next");
                }
            }
        }

        private sealed class ConcurrentRecordingObserver : IObserver<IState>
        {
            private readonly Lock _gate = new();
            private readonly List<int> _counts = [];
            private int _completedCount;
            private Exception? _error;

            public int CompletedCount
            {
                get
                {
                    lock (_gate)
                    {
                        return _completedCount;
                    }
                }
            }

            public Exception? Error
            {
                get
                {
                    lock (_gate)
                    {
                        return _error;
                    }
                }
            }

            public int[] GetCounts()
            {
                lock (_gate)
                {
                    return [.. _counts];
                }
            }

            public void OnCompleted()
            {
                lock (_gate)
                {
                    _completedCount++;
                }
            }

            public void OnError(Exception error)
            {
                lock (_gate)
                {
                    _error = error;
                }
            }

            public void OnNext(IState value)
            {
                lock (_gate)
                {
                    _counts.Add(((CounterState)value).Count);
                }
            }
        }
    }
}