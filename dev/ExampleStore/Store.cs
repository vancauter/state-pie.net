using AtomicRx;

namespace Dev.ExampleStore
{
    public static class Store
    {
        public static IStore Create()
        {
            var counterSlice = Slices.CreateCounterSlice();
            var personSlice = Slices.CreatePersonSlice();
            var nestedSlice = Slices.CreateNestedSlice();

            var (state, reducer) = Factory.CombineReducers(counterSlice, personSlice, nestedSlice);

            return Factory.CreateStore(state, reducer);
        }
    }
}
