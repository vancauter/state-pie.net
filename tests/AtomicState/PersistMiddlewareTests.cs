using SC.AtomicState.Persistence;

namespace SC.AtomicState.Tests
{
    public class PersistMiddlewareTests
    {
        private sealed record TestState(int Value) : IState;

        [Fact]
        public async Task PersistMiddleware_SavesSnapshot_SyncMode()
        {
            var snapshotStore = new InMemorySnapshotStore();
            TestState initial = new(0);

            IStore? store = null;

            var middleware = PersistMiddleware.Create(snapshotStore, "sync-key", () => store!.State, saveInBackground: false);

            store = Factory.CreateStore(initial, (state, action) =>
            {
                if (action.Type == "INC") return new TestState(((TestState)state).Value + 1);
                return state;
            }, middleware);

            store.Dispatch(Factory.CreateAction("INC"));

            var loaded = await snapshotStore.LoadAsync("sync-key");

            Assert.NotNull(loaded);
            Assert.Same(store.State, loaded);
        }

        [Fact]
        public async Task PersistMiddleware_SavesSnapshot_BackgroundMode()
        {
            var snapshotStore = new InMemorySnapshotStore();
            TestState initial = new(10);

            IStore? store = null;

            var middleware = PersistMiddleware.Create(snapshotStore, "bg-key", () => store!.State, saveInBackground: true);

            store = Factory.CreateStore(initial, (state, action) =>
            {
                if (action.Type == "DEC") return new TestState(((TestState)state).Value - 1);
                return state;
            }, middleware);

            store.Dispatch(Factory.CreateAction("DEC"));

            // Poll until snapshot appears or timeout
            IState? loaded = null;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(2))
            {
                loaded = await snapshotStore.LoadAsync("bg-key");
                if (loaded is not null) break;
                await Task.Delay(10);
            }

            Assert.NotNull(loaded);
            Assert.Equal(store.State.GetType(), loaded.GetType());
        }

        [Fact]
        public void PersistMiddleware_SwallowsSaveExceptions()
        {
            var throwing = new ThrowingSnapshotStore(throwOnSave: true, throwOnLoad: false);
            TestState initial = new(5);

            IStore? store = null;

            var middleware = PersistMiddleware.Create(throwing, "err-key", () => store!.State, saveInBackground: false);

            store = Factory.CreateStore(initial, (state, action) => state, middleware);

            // Dispatch should not throw despite the snapshot store throwing
            store.Dispatch(Factory.CreateAction("NOOP"));
        }
    }
}

