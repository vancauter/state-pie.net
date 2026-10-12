using Xunit;

namespace SnapshotStore.Tests
{
    public class StoreSubscriptionTests
    {
        [Fact]
        public void GivenCounterSelectorSubscription_WhenCounterChanges_ThenOnlyChangedValuesAreDelivered()
        {
            using var store = CreateCounterStore();
            var selector = StoreFactory.CreateSelector(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                static state => state.Count);
            var receivedValues = new List<int>();
            using var subscription = store.Subscribe(selector, receivedValues.Add);

            store.Dispatch(new IncrementCounterAction());
            store.Dispatch(new IncrementCounterAction());

            Assert.Equal([1, 2], receivedValues);
        }

        [Fact]
        public void GivenCounterSelectorSubscription_WhenAnotherSliceChanges_ThenNoValueIsDelivered()
        {
            var counterSlice = CreateAppCounterSlice();
            var statusSlice = StoreFactory.CreateSlice<StatusState, IAppAction>(
                name: "status",
                initialState: new StatusState("idle"),
                reducer: static (state, action) => action is SetStatusAction set
                    ? state with { Value = set.Value }
                    : state);
            using var twoSliceStore = StoreFactory.CreateStore(counterSlice, statusSlice);
            var selectedValues = new List<int>();
            using var subscription = twoSliceStore.Subscribe(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                state => selectedValues.Add(state.Count));

            twoSliceStore.Dispatch(new SetStatusAction("ready"));

            Assert.Empty(selectedValues);
            Assert.Equal(0, Assert.IsType<CounterState>(twoSliceStore.State["counter"]).Count);
        }

        [Fact]
        public void GivenCustomComparer_WhenSelectedValuesCompareEqual_ThenListenerIsNotCalled()
        {
            var themeSlice = StoreFactory.CreateSlice<ThemeState, IThemeAction>(
                name: "theme",
                initialState: new ThemeState("dark"),
                reducer: static (state, action) => action is SetThemeAction set
                    ? state with { Value = set.Value }
                    : state);
            using var store = StoreFactory.CreateStore(themeSlice);
            var receivedValues = new List<string>();
            using var subscription = store.Subscribe(
                StoreFactory.CreateSelector(
                    StoreFactory.CreateSliceSelector<ThemeState>("theme"),
                    static state => state.Value),
                receivedValues.Add,
                StringComparer.OrdinalIgnoreCase);

            store.Dispatch(new SetThemeAction("DARK"));

            Assert.Empty(receivedValues);
            Assert.Equal("DARK", Assert.IsType<ThemeState>(store.State["theme"]).Value);
        }

        [Fact]
        public void GivenDisposedSubscription_WhenStateChanges_ThenListenerIsNotCalled()
        {
            using var store = CreateCounterStore();
            var receivedValues = new List<int>();
            var subscription = store.Subscribe(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                state => receivedValues.Add(state.Count));
            subscription.Dispose();

            store.Dispatch(new IncrementCounterAction());

            Assert.Empty(receivedValues);
        }

        [Fact]
        public void GivenNullSelectorOrListener_WhenSubscribing_ThenArgumentNullExceptionIsThrown()
        {
            using var store = CreateCounterStore();
            var selector = StoreFactory.CreateSliceSelector<CounterState>("counter");
            static void listener(CounterState _) { }

            _ = Assert.Throws<ArgumentNullException>(() => store.Subscribe<CounterState>(null!, listener));
            _ = Assert.Throws<ArgumentNullException>(() => store.Subscribe(selector, null!));
        }

        [Fact]
        public void GivenSelectorThrowsForCandidateState_WhenDispatching_ThenStateAndSubscriptionBaselineRemainUnchanged()
        {
            using var store = CreateCounterStore();
            static int selector(IReadOnlyDictionary<string, IState> snapshot)
            {
                var state = (CounterState)snapshot["counter"];
                if (state.Count == 1)
                    throw new InvalidOperationException("Selector failed.");

                return state.Count;
            }
            var receivedValues = new List<int>();
            using var subscription = store.Subscribe(selector, receivedValues.Add);
            var before = store.State;

            _ = Assert.Throws<InvalidOperationException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Same(before, store.State);
            Assert.Empty(receivedValues);
        }

        [Fact]
        public void GivenListenerThrows_WhenSelectedValueChanges_ThenExceptionPropagatesAfterStateCommit()
        {
            using var store = CreateCounterStore();
            using var subscription = store.Subscribe(
                StoreFactory.CreateSliceSelector<CounterState>("counter"),
                static _ => throw new InvalidOperationException("Listener failed."));

            var exception = Assert.Throws<InvalidOperationException>(() => store.Dispatch(new IncrementCounterAction()));

            Assert.Equal("Listener failed.", exception.Message);
            Assert.Equal(1, Assert.IsType<CounterState>(store.State["counter"]).Count);
        }

        [Fact]
        public void GivenDisposedStore_WhenSubscribing_ThenObjectDisposedExceptionIsThrown()
        {
            var store = CreateCounterStore();
            store.Dispose();

            _ = Assert.Throws<ObjectDisposedException>(() =>
                store.Subscribe(StoreFactory.CreateSliceSelector<CounterState>("counter"), static _ => { }));
        }

        private static IStore<ICounterAction> CreateCounterStore() => StoreFactory.CreateStore(CreateCounterSlice());

        private static ISlice<ICounterAction> CreateCounterSlice() =>
            StoreFactory.CreateSlice<CounterState, ICounterAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action is IncrementCounterAction
                    ? state with { Count = state.Count + 1 }
                    : state);

        private static ISlice<IAppAction> CreateAppCounterSlice() =>
            StoreFactory.CreateSlice<CounterState, IAppAction>(
                name: "counter",
                initialState: new CounterState(0),
                reducer: static (state, action) => action is IncrementCounterAction
                    ? state with { Count = state.Count + 1 }
                    : state);

        private sealed record CounterState(int Count) : IState;

        private sealed record StatusState(string Value) : IState;

        private sealed record ThemeState(string Value) : IState;

        private interface ICounterAction : IAction;

        private interface IAppAction : IAction;

        private interface IThemeAction : IAction;

        private sealed record IncrementCounterAction : ICounterAction, IAppAction;

        private sealed record SetStatusAction(string Value) : IAppAction;

        private sealed record SetThemeAction(string Value) : IThemeAction;
    }
}
