namespace SC.AtomicState
{
    /// <summary>
    /// Represents a reusable slice identity that can participate in reducer composition.
    /// </summary>
    public abstract class SliceHandle
    {
        internal abstract ISlice Slice { get; }

        /// <summary>
        /// Gets the unique slice name used inside a combined root state.
        /// </summary>
        public string Name => Slice.Name;

        /// <summary>
        /// Gets the setup-time state snapshot for the slice.
        /// </summary>
        public IState InitialState => Slice.InitialState;

        /// <summary>
        /// Gets the reducer that computes the next slice state.
        /// </summary>
        public Reducer Reducer => Slice.Reducer;
    }

    /// <summary>
    /// Represents a strongly typed slice identity that callers can carry from creation through state access.
    /// </summary>
    /// <typeparam name="TState">The concrete state type produced by the slice.</typeparam>
    public sealed class SliceHandle<TState> : SliceHandle where TState : IState
    {
        private readonly ISlice _slice;

        internal SliceHandle(ISlice slice)
        {
            ArgumentNullException.ThrowIfNull(slice);
            _slice = slice;
        }

        internal override ISlice Slice => _slice;
    }
}
