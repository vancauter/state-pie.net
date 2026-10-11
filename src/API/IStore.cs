namespace SnapshotStore
{
    /// <summary>
    /// Represents a typed immutable store where dispatch accepts only one action family and publishes the latest state snapshot.
    /// </summary>
    public interface IStore<TAction> : IDisposable where TAction : class, IAction
    {
        StateSnapshot State { get; }

        event Action<StateSnapshot> OnStateChanged;

        void Dispatch(TAction action);
    }
}