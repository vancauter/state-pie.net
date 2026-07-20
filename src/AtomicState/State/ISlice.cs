namespace SC.AtomicState
{
    /// <summary>
    /// Represents the internal slice contract used during combined-state composition.
    /// </summary>
    /// <remarks>
    /// Public callers normally work with <see cref="SliceHandle"/> values returned by <see cref="Factory.CreateSlice{TState}(string, TState, Reducer{TState})"/>.
    /// The factory uses this contract internally when it builds a combined root state from those handles.
    /// </remarks>
    public interface ISlice
    {
        /// <summary>
        /// Gets the unique slice name used as the key in the combined root state.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the initial state snapshot for the slice.
        /// </summary>
        IState InitialState { get; }

        /// <summary>
        /// Gets the reducer that computes the next slice state.
        /// </summary>
        Reducer Reducer { get; }
    }
}