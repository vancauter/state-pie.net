namespace StatePie
{
    public static partial class StoreFactory
    {
        /// <summary>
        /// Factory method for creating a slice.
        /// </summary>
        public static ISlice<TAction> CreateSlice<TState, TAction>(string name, TState initialState, Reducer<TState, TAction> reducer)
            where TState : IState
            where TAction : class, IAction
        {
            // Name validation is strict so state lookups are stable and diagnostics stay clear.
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(name));

            // Null initial state would fail unpredictably later during reducer execution.
            if (initialState is null)
                throw new ArgumentNullException(nameof(initialState));

            // Explicit delegate validation surfaces wiring errors at composition time.
            if (reducer is null)
                throw new ArgumentNullException(nameof(reducer));

            // Creates a typed slice wrapper
            return new Slice<TState, TAction>(name, initialState, reducer);
        }
    }
}
