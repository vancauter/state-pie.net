namespace StateContainer
{
    public interface IAction
    {
        string Type { get; }
    }

    public interface IAction<TPayload> : IAction
    {
        TPayload Payload { get; }
    }

    public interface IState { }

    public interface ISlice
    {
        string Name { get; }
        IState InitialState { get; }
        Reducer ReducerFn { get; }
    }

    public interface IStateContainer : IDisposable
    {
        IObservable<IState> StateChanged { get; }

        IState State { get; }

        void Dispatch(IAction action);
    }
}
