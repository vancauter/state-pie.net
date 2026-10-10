namespace SnapshotStore
{
    public interface IStore : IDisposable
    {
        IReadOnlyDictionary<string, IState> State { get; }

        event System.Action<IReadOnlyDictionary<string, IState>> OnStateChanged;

        void Dispatch(IAction action);
    }
}