namespace StatePie
{
    /// <summary>
    /// Defines the reducer shape used by store composition when state is handled through the shared IState surface and actions remain strongly typed.
    /// </summary>
    public delegate IState Reducer<TAction>(IState state, TAction action)
        where TAction : class, IAction;

    /// <summary>
    /// Defines the reducer shape for concrete slice state so transitions stay type-safe and resilient to refactoring.
    /// </summary>
    public delegate TState Reducer<TState, TAction>(TState state, TAction action)
        where TState : IState
        where TAction : class, IAction;

    /// <summary>
    /// Defines the root reducer shape that combines all slices while preserving typed action dispatch.
    /// </summary>
    public delegate StateSnapshot RootReducer<TAction>(StateSnapshot state, TAction action)
        where TAction : class, IAction;
}