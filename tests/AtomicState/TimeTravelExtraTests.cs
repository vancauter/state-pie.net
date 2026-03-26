using SC.AtomicState.MiddlewareEx;

namespace SC.AtomicState.Tests
{
    public class TimeTravelExtraTests
    {
        private sealed record TestState(int Value) : IState;

        [Fact]
        public void History_Grows_When_MaxHistory_NotEnforced()
        {
            var options = new TimeTravelOptions(MaxHistory: 2);

            var initial = new TestState(0);

            IStore? store = null;

            var middleware = SC.AtomicState.MiddlewareEx.TimeTravelMiddleware.Create(options, () => store!.State, state => SC.AtomicState.Persistence.StoreSnapshotHelper.SeedStoreAsync(store!, state), out var controller);

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "INC") return new TestState(((TestState)state).Value + 1);
                return state;
            });

            store = Factory.CreateStore(initial, reducer, middleware);

            // Dispatch more times than MaxHistory; current implementation does not evict
            for (var i = 0; i < 5; i++) store.Dispatch(Factory.CreateAction("INC"));

            var history = controller.GetHistory();
            Assert.Equal(5, history.Count);
        }

        [Fact]
        public async Task ReplayAsync_IsNoopAndDoesNotThrow()
        {
            var options = new TimeTravelOptions();

            var initial = new TestState(0);

            IStore? store = null;

            var middleware = SC.AtomicState.MiddlewareEx.TimeTravelMiddleware.Create(options, () => store!.State, state => SC.AtomicState.Persistence.StoreSnapshotHelper.SeedStoreAsync(store!, state), out var controller);

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "INC") return new TestState(((TestState)state).Value + 1);
                return state;
            });

            store = Factory.CreateStore(initial, reducer, middleware);

            store.Dispatch(Factory.CreateAction("INC"));

            // Build a replay action list (this will not be applied by current ReplayAsync implementation)
            var actions = new List<IAction> { Factory.CreateAction("INC") };

            await controller.ReplayAsync(actions);

            // Store state should remain at 1 since ReplayAsync is a noop in this implementation
            Assert.Equal(1, ((TestState)store.State).Value);
        }
    }
}
