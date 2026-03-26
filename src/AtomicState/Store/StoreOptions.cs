namespace SC.AtomicState
{
    [Flags]
    public enum DebugGuards
    {
        None = 0,
        WarnOnLongDispatch = 1,
        DetectBlockingWaits = 2
    }

    public record StoreOptions(
        DebugGuards DebugGuards = DebugGuards.None,
        TimeSpan DispatchGuardThreshold = default,
        Action<string>? OnDebugWarning = null
    );
}
