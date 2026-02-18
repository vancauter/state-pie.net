namespace SCLibs.StateContainer
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
        IState State { get; }
        ReducerFn Reducer { get; }
    }

    public interface IStore : IDisposable
    {
        IState State { get; }
        IObservable<IState> StateChanged { get; }
        void Dispatch(IAction action);
    }
}
