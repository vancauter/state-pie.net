namespace StatePie
{
    /// <summary>
    /// Adapts concrete typed slice reducers to the store's shared IState composition surface without losing action-type safety.
    /// </summary>
    internal sealed class Slice<TState, TAction> : ISlice<TAction>
        where TState : IState
        where TAction : class, IAction
    {
        /// <summary>
        /// Creates a named slice adapter with initial state and a typed reducer.
        /// </summary>
        public Slice(string sliceName, TState initialState, Reducer<TState, TAction> reducer)
        {
            Name = sliceName;
            InitialState = initialState;

            // The store composes slices through IState for uniformity, but each slice still has a
            // concrete state type. This adapter keeps composition simple while preserving type safety.
            Reducer = (state, action) =>
            {
                // Explicit type guard makes mismatched slice wiring fail with actionable diagnostics.
                if (state is not TState typedState)
                {
                    throw new InvalidOperationException(
                        $"Slice '{sliceName}' expected state type '{typeof(TState).Name}' but received '{state.GetType().Name}'.");
                }

                // After guard, reducer executes on fully typed state and action family.
                return reducer(typedState, action);
            };
        }

        public string Name { get; }

        public IState InitialState { get; }

        public Reducer<TAction> Reducer { get; }
    }
}
