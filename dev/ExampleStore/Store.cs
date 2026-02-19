using AtomicRx;

namespace Dev.ExampleStore
{
    public static class Store
    {
        // Allow callers to supply optional middleware when creating the example store.
        public static IStore Create(params Middleware[] middleware)
        {
            var counterSlice = Slices.CreateCounterSlice();
            var personSlice = Slices.CreatePersonSlice();

            var (state, reducer) = Factory.CombineReducers(counterSlice, personSlice);

            return Factory.CreateStore(state, reducer, middleware);
        }
    }
}
