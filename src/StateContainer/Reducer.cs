namespace SCLibs.StateContainer
{
    public delegate IState Reducer(IState state, IAction action);
}
