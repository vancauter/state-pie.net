using SC.AtomicState.MiddlewareEx;

namespace SC.AtomicState.Tests
{
    public class TimetravelTests
    {
        private sealed record TestState(int Value) : IState;

        [Fact]
        public async Task Timetravel_RecordUndoRedo_Works()
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
            store.Dispatch(Factory.CreateAction("INC"));

            var history = controller.GetHistory();
            // Two commits recorded
            Assert.Equal(2, history.Count);

            // Undo once
            await controller.UndoAsync();
            Assert.Equal(1, ((TestState)store.State).Value);

            // Redo
            await controller.RedoAsync();
            Assert.Equal(2, ((TestState)store.State).Value);
        }
    }
}
