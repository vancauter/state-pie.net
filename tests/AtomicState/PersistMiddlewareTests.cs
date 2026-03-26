using System.Threading.Tasks;
using SC.AtomicState.Persistence;

namespace SC.AtomicState.Tests
{
    public class PersistMiddlewareTests
    {
        private sealed record TestState(int Count) : IState;

        [Fact]
        public async Task PersistMiddleware_SavesSnapshot_Synchronously_WhenConfigured()
        {
            var snapshotStore = new InMemorySnapshotStore();
            IStore? store = null;

            var middleware = PersistMiddleware.Create(snapshotStore, "k", () => store!.State, saveInBackground: false);

            store = Factory.CreateStore(new TestState(0), (s, a) => a.Type == "inc" ? new TestState(((TestState)s).Count + 1) : s, middleware);

            store.Dispatch(Factory.CreateAction("inc"));

            var loaded = await snapshotStore.LoadAsync("k");

            Assert.NotNull(loaded);
            Assert.Equal(((TestState)store.State).Count, ((TestState)loaded!).Count);
        }

        [Fact]
        public async Task PersistMiddleware_SavesSnapshot_InBackground_WhenConfigured()
        {
            var snapshotStore = new InMemorySnapshotStore();
            IStore? store = null;

            var middleware = PersistMiddleware.Create(snapshotStore, "kb", () => store!.State, saveInBackground: true);

            store = Factory.CreateStore(new TestState(0), (s, a) => a.Type == "inc" ? new TestState(((TestState)s).Count + 1) : s, middleware);

            store.Dispatch(Factory.CreateAction("inc"));

            // Wait a short period for background save. In production this is background; tests allow a small grace.
            await Task.Delay(50);

            var loaded = await snapshotStore.LoadAsync("kb");

            Assert.NotNull(loaded);
            Assert.Equal(((TestState)store.State).Count, ((TestState)loaded!).Count);
        }
    }
}
