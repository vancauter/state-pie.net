namespace AtomicRx
{
    /// <summary>
    /// Marks a type as valid store state.
    /// </summary>
    /// <remarks>
    /// State values are expected to behave as immutable snapshots so reducers can rely on reference-based change detection.
    /// </remarks>
    public interface IState
    {
    }
}