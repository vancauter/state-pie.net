namespace SCLibs.StateContainer
{
    // Non-generic version for general use.
    public delegate IState ReducerFn(IState state, IAction action);

    // Generic version for type safety within slices.
    public delegate TState ReducerFn<TState>(TState state, IAction action) where TState : IState;
}