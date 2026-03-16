namespace AtomicRx
{
    /// <summary>
    /// Represents a reducer that computes the next state from the current state and a dispatched action.
    /// </summary>
    /// <param name="state">The current root state.</param>
    /// <param name="action">The dispatched action to handle.</param>
    /// <returns>The next root state after applying the action.</returns>
    /// <remarks>
    /// Reducers should be pure and should return the existing state instance when no change is required.
    /// </remarks>
    public delegate IState Reducer(IState state, IAction action);

    /// <summary>
    /// Represents a reducer specialized for a concrete slice state type.
    /// </summary>
    /// <typeparam name="TState">The concrete state type for the slice.</typeparam>
    /// <param name="state">The current slice state.</param>
    /// <param name="action">The dispatched action to handle.</param>
    /// <returns>The next slice state after applying the action.</returns>
    public delegate TState Reducer<TState>(TState state, IAction action) where TState : IState;
}