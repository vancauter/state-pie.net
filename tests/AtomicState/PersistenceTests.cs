using System.Threading.Tasks;
using SC.AtomicState.Persistence;

namespace SC.AtomicState.Tests
{
    public class PersistenceTests
    {
        private sealed record TestState(int Value) : IState;

        [Fact]
        public async Task InMemorySnapshotStore_SaveAndLoad_ReturnsSameInstance()
        {
            var store = new InMemorySnapshotStore();
            var state = new TestState(42);

            await store.SaveAsync("key", state);

            var loaded = await store.LoadAsync("key");

            Assert.Same(state, loaded);
        }

        [Fact]
        public async Task InMemorySnapshotStore_LoadMissing_ReturnsNull()
        {
            var store = new InMemorySnapshotStore();

            var loaded = await store.LoadAsync("missing");

            Assert.Null(loaded);
        }

        [Fact]
        public async Task ThrowingSnapshotStore_Save_Throws()
        {
            var t = new ThrowingSnapshotStore(throwOnSave: true, throwOnLoad: false);

            await Assert.ThrowsAsync<InvalidOperationException>(() => t.SaveAsync("k", new TestState(1)));
        }

        [Fact]
        public async Task ThrowingSnapshotStore_Load_Throws()
        {
            var t = new ThrowingSnapshotStore(throwOnSave: false, throwOnLoad: true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => t.LoadAsync("k"));
        }
    }
}
