namespace AtomicRx.Tests.TestStore
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

        // A shared singleton store handy for tests that don't need isolated instances.
        public static IStore Instance { get; private set; } = Create();

        // Reset recreates the singleton instance, disposing the previous one if applicable.
        public static void Reset()
        {
            if (Instance is IDisposable d) d.Dispose();
            Instance = Create();
        }
    }
}
