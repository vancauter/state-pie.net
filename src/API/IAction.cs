namespace SnapshotStore
{
    /// <summary>
    /// Represents an action that can be dispatched to the store to trigger state changes.
    /// </summary>
    public interface IAction
    {
        string Type { get; }
    }

    /// <summary>
    /// Represents an action that can be dispatched to the store to trigger state changes, with a payload of type T.
    /// </summary>
    public interface IAction<T> : IAction where T : notnull
    {
        T Payload { get; }
    }
}