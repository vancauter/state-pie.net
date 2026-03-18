namespace SC.AtomicState
{
    /// <summary>
    /// Represents a reducer that computes the next root state from the current state and a dispatched action.
    /// </summary>
    /// <param name="state">The current root state.</param>
    /// <param name="action">The dispatched action to handle.</param>
    /// <returns>The next root state after applying the action.</returns>
    /// <remarks>
    /// Reducers should be pure and should return the existing <paramref name="state"/> instance when no change is
    /// required. AtomicState treats a new state reference as a committed update, even when the replacement is
    /// value-equal to the current state.
    /// </remarks>
    public delegate IState Reducer(IState state, IAction action);

    /// <summary>
    /// Represents a reducer specialized for a concrete slice state type.
    /// </summary>
    /// <typeparam name="TState">The concrete state type for the slice.</typeparam>
    /// <param name="state">The current slice state.</param>
    /// <param name="action">The dispatched action to handle.</param>
    /// <returns>The next slice state after applying the action.</returns>
    /// <remarks>
    /// Implementations should return the existing <paramref name="state"/> instance when no slice update is required.
    /// Returning a new but logically equivalent slice instance defeats the store's reference-based no-op detection.
    /// </remarks>
    public delegate TState Reducer<TState>(TState state, IAction action) where TState : IState;
}