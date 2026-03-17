namespace AtomicRx
{
    /// <summary>
    /// Represents a named slice that participates in a combined store state.
    /// </summary>
    /// <remarks>
    /// Each slice contributes an initial <see cref="State"/> value and a <see cref="Reducer"/> that is invoked by <see cref="Factory.CombineReducers(ISlice[])"/>.
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
        IState State { get; }

        /// <summary>
        /// Gets the reducer that computes the next slice state.
        /// </summary>
        Reducer Reducer { get; }
    }
}