namespace SC.AtomicState.Telemetry
{
    /// <summary>
    /// Lightweight hooks for store-level metrics and middleware timing.
    /// </summary>
    public interface IStoreMetrics
    {
        void OnDispatchStart();
        void OnDispatchEnd(TimeSpan elapsed);
        void OnMiddlewareStart(string middlewareId);
        void OnMiddlewareEnd(string middlewareId, TimeSpan elapsed);
    }
}
