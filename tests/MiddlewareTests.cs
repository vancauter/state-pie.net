using AtomicRx.Tests.TestStore;

namespace AtomicRx.Tests
{
    public class MiddlewareTests
    {
        // Test Thunk Middleware Executes Thunk Payload
        // Why: Verify that async thunk payloads (Func<IStore, Task>) are invoked by the middleware
        //       and that actions dispatched from the thunk update the store state asynchronously.
        // What: Dispatch an async thunk which dispatches two "INC" actions with a short delay,
        //       then assert the aggregated state contains the expected incremented value.
        [Fact]
        public async Task ThunkMiddlewareExecutesThunkPayload()
        {
            // reducer: handle "INC" action by returning new CounterState
            var slice = Slices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            using var store = Factory.CreateStore(rootState, rootReducer, ThunkMiddleware.Create());

            // dispatch a thunk that will dispatch two INC actions
            var thunk = new Func<IStore, Task>(async s =>
            {
                s.Dispatch(Factory.CreateAction(Slices.Increment));
                await Task.Delay(10);
                s.Dispatch(Factory.CreateAction(Slices.Increment));
            });

            store.Dispatch(Factory.CreateAction("THUNK", thunk));

            // allow async thunk to run
            await Task.Delay(50);

            // Use public serializer to inspect the root state without casting to internal types.
            var serialized = Helpers.SerializeState(store.State);
            Assert.Contains("2", serialized);
        }

        // Test Thunk Middleware Executes Sync Thunk Payload
        // Why: Verify that synchronous thunk payloads (Action<IStore>) are invoked immediately
        //       and that their inner dispatches synchronously update the store state.
        // What: Dispatch a sync thunk that dispatches two "INC" actions and assert the state reflects both increments.
        [Fact]
        public void ThunkMiddlewareExecutesSyncThunkPayload()
        {
            var slice = Slices.CreateCounterSlice();
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            using var store = Factory.CreateStore(rootState, rootReducer, ThunkMiddleware.Create());

            // dispatch a synchronous thunk that will dispatch two INC actions
            var thunk = new Action<IStore>(s =>
            {
                s.Dispatch(Factory.CreateAction(Slices.Increment));
                s.Dispatch(Factory.CreateAction(Slices.Increment));
            });

            store.Dispatch(Factory.CreateAction("THUNK", thunk));

            // dispatches are synchronous; state should be updated immediately
            var serialized = Helpers.SerializeState(store.State);
            Assert.Contains("2", serialized);
        }

        // Test Logging Middleware Records Actions Including Thunk Inner Dispatches
        // Why: Ensure that logging middleware records actions in the correct order, including those
        //       dispatched from inside a thunk, so middleware observes the entire dispatch sequence.
        // What: Compose logging + thunk middleware, dispatch a thunk that emits two "INC" actions,
        //       then assert the recorded log sequence matches the expected order including the original thunk action.
        [Fact]
        public async Task LoggingMiddlewareRecordsActionsIncludingThunkInnerDispatches()
        {
            var slice = Slices.CreateCounterSlice();

            var logs = new List<string>();
            var logging = LoggingMiddleware.Create(logs);

            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            using var store = Factory.CreateStore(rootState, rootReducer, logging, ThunkMiddleware.Create());

            var thunk = new Func<IStore, Task>(async s =>
            {
                s.Dispatch(Factory.CreateAction(Slices.Increment));
                await Task.Delay(5);
                s.Dispatch(Factory.CreateAction(Slices.Increment));
            });

            store.Dispatch(Factory.CreateAction("THUNK", thunk));
            await Task.Delay(50);

            // Expect log sequence: THUNK (original action), then two increment actions dispatched by the thunk
            var normalized = logs.Select(l => l == "INC" ? Slices.Increment : l).ToArray();
            var expected = new[] { "THUNK", Slices.Increment, Slices.Increment };
            Assert.Equal(expected, normalized);
        }
    }
}
