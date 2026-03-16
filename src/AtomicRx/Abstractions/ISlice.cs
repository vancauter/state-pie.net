namespace AtomicRx
{
    /// <summary>
    /// Represents a named slice of a combined store state.
    /// </summary>
    /// <remarks>
    /// Each slice contributes an initial <see cref="State"/> value and a <see cref="Reducer"/> that is invoked by <see cref="Factory.CombineReducers(ISlice[])"/>.
    /// </remarks>
    public interface ISlice
    {
        /// <summary>
        /// Gets the unique slice name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the initial state for the slice.
        /// </summary>
        IState State { get; }

        /// <summary>
        /// Gets the reducer that computes the next slice state.
        /// </summary>
        Reducer Reducer { get; }
    }
}