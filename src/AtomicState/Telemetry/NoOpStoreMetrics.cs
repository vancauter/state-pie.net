namespace SC.AtomicState.Telemetry
{
    /// <summary>
    /// Default no-op implementation of <see cref="IStoreMetrics"/>.
    /// </summary>
    public sealed class NoOpStoreMetrics : IStoreMetrics
    {
        public void OnDispatchStart() { }
        public void OnDispatchEnd(TimeSpan elapsed) { }
        public void OnMiddlewareStart(string middlewareId) { }
        public void OnMiddlewareEnd(string middlewareId, TimeSpan elapsed) { }
    }
}
