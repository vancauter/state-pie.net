namespace SCLibs.StateContainer
{
    /// <summary>
    /// Reducer delegate that accepts the current root <see cref="IState"/> and an <see cref="IAction"/>
    /// and returns the next root state. Implementations should return the same instance when no change occurs.
    /// </summary>
    /// <param name="state">The current root state.</param>
    /// <param name="action">The dispatched action to handle.</param>
    /// <returns>The next root state after applying the action.</returns>
    /// <remarks>
    /// Reducers must be pure functions with no side effects.
    /// They should not mutate the input state but instead return a new state instance when changes occur.
    /// When no changes are needed, they should return the original state instance to allow for efficient change detection.
    /// </remarks>
    public delegate IState Reducer(IState state, IAction action);

    /// <summary>
    /// Generic reducer delegate for a specific slice state type.
    /// Provides compile-time safety for reducers that work with a concrete <typeparamref name="TState"/>.
    /// </summary>
    /// <typeparam name="TState">The concrete state type for the slice.</typeparam>
    /// <inheritdoc cref="Reducer"/>
    public delegate TState Reducer<TState>(TState state, IAction action) where TState : IState;
}