using AtomicRx;

namespace Dev.ExampleStore
{
    public static class ExampleStoreFactory
    {
        public static IStore Create(params AtomicMiddleware[] middleware)
        {
            var counterSlice = ExampleSlices.CreateCounterSlice();
            var personSlice = ExampleSlices.CreatePersonSlice();

            var (state, reducer) = Factory.CombineReducers(counterSlice, personSlice);

            return Factory.CreateStore(state, reducer, middleware);
        }
    }
}