using AtomicRx;

namespace StateContainer.Tests
{
    public record CounterState(int Count) : IState;

    public class MiddlewareTests
    {
        [Fact]
        public async Task ThunkMiddlewareExecutesThunkPayload()
        {
            // reducer: handle "INC" action by returning new CounterState
            static CounterState reducer(CounterState state, IAction action)
            {
                if (action.Type == "INC") return new CounterState(state.Count + 1);
                return state;
            }

            var slice = Factory.CreateSlice("counter", new CounterState(0), reducer);
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            using var store = Factory.CreateStore(rootState, rootReducer, ThunkMiddleware.Create());

            // dispatch a thunk that will dispatch two INC actions
            var thunk = new Func<IStore, Task>(async s =>
            {
                s.Dispatch(Factory.CreateAction("INC"));
                await Task.Delay(10);
                s.Dispatch(Factory.CreateAction("INC"));
            });

            store.Dispatch(Factory.CreateAction("THUNK", thunk));

            // allow async thunk to run
            await Task.Delay(50);

            // Use public serializer to inspect the root state without casting to internal types.
            var serialized = Helpers.SerializeState(store.State);
            Assert.Contains("2", serialized);
        }

        private static readonly string[] s_expected = ["THUNK", "INC", "INC"];

        [Fact]
        public async Task LoggingMiddlewareRecordsActionsIncludingThunkInnerDispatches()
        {
            static CounterState reducer(CounterState state, IAction action)
            {
                if (action.Type == "INC") return new CounterState(state.Count + 1);
                return state;
            }

            var slice = Factory.CreateSlice("counter", new CounterState(0), reducer);
            var (rootState, rootReducer) = Factory.CombineReducers(slice);

            var logs = new List<string>();
            var logging = LoggingMiddleware.Create(logs);

            using var store = Factory.CreateStore(rootState, rootReducer, logging, ThunkMiddleware.Create());

            var thunk = new Func<IStore, Task>(async s =>
            {
                s.Dispatch(Factory.CreateAction("INC"));
                await Task.Delay(5);
                s.Dispatch(Factory.CreateAction("INC"));
            });

            store.Dispatch(Factory.CreateAction("THUNK", thunk));
            await Task.Delay(50);

            // Expect log sequence: THUNK (original action), then two INC actions dispatched by the thunk
            Assert.Equal(s_expected, logs);
        }
    }
}
