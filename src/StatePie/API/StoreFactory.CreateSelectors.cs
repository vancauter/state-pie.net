namespace StatePie
{
    public static partial class StoreFactory
    {
        /// <summary>
        /// Factory method for creating a typed selector for a single slice key.
        /// </summary>
        /// <typeparam name="TState">The expected type of the slice state.</typeparam>
        /// <param name="sliceName">The identifier key of the slice.</param>
        /// <returns>A selector function extracting the typed slice state from a snapshot.</returns>
        public static Func<StateSnapshot, TState> CreateSliceSelector<TState>(string sliceName)
            where TState : IState
        {
            if (string.IsNullOrWhiteSpace(sliceName))
                throw new ArgumentException("Slice name must be a non-empty string.", nameof(sliceName));

            return snapshot =>
            {
                if (snapshot is null)
                    throw new ArgumentNullException(nameof(snapshot));

                if (!snapshot.TryGetValue(sliceName, out var state))
                    throw new KeyNotFoundException($"Slice key '{sliceName}' was not found in the current state snapshot.");

                if (state is not TState typedState)
                    throw new InvalidOperationException($"Slice key '{sliceName}' was expected to be '{typeof(TState).Name}' but was '{state.GetType().Name}'.");

                return typedState;
            };
        }

        /// <summary>
        /// Factory method for creating a projected selector from an existing source selector.
        /// </summary>
        /// <typeparam name="TSource">The type of the source state or value.</typeparam>
        /// <typeparam name="TResult">The type of the projected result.</typeparam>
        /// <param name="sourceSelector">The source selector function.</param>
        /// <param name="projector">The projection function applied to the source value.</param>
        /// <returns>A new selector function returning the projected result.</returns>
        public static Func<StateSnapshot, TResult> CreateSelector<TSource, TResult>(Func<StateSnapshot, TSource> sourceSelector, Func<TSource, TResult> projector)
        {
            if (sourceSelector is null)
                throw new ArgumentNullException(nameof(sourceSelector));

            if (projector is null)
                throw new ArgumentNullException(nameof(projector));

            return snapshot => projector(sourceSelector(snapshot));
        }
    }
}
