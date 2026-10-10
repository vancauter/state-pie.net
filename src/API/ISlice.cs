namespace SnapshotStore
{
    public interface ISlice
    {
        string Name { get; }

        IState InitialState { get; }

        Reducer Reducer { get; }
    }
}