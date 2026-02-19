using AtomicRx;

namespace Dev.ExampleStore
{
    public static class Store
    {
        public static IStore Create()
        {
            var counterSlice = Slices.CreateCounterSlice();
            var personSlice = Slices.CreatePersonSlice();

            var (state, reducer) = Factory.CombineReducers(counterSlice, personSlice);

            return Factory.CreateStore(state, reducer);
        }
    }
}
