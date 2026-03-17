namespace SC.AtomicState.Tests.Shared
{
    public static class TestStoreFactory
    {
        public static IStore Create()
        {
            var counterSlice = TestSlices.CreateCounterSlice();
            var personSlice = TestSlices.CreatePersonSlice();
            var nestedSlice = TestSlices.CreateNestedSlice();

            var (state, reducer) = Factory.CombineReducers(counterSlice, personSlice, nestedSlice);

            return Factory.CreateStore(state, reducer);
        }
    }
}