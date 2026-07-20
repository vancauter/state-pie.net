/*
 * Example 09 - Metrics collector
 * Purpose:
 *   Demonstrates injecting a minimal `IStoreMetrics` implementation to observe
 *   dispatch lifecycle events for testing or simple telemetry. Production code
 *   should forward metrics to a robust telemetry system.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using System.Threading;
using SC.AtomicState;
using SC.AtomicState.Telemetry;

namespace Examples
{
    public sealed record MState(int Value) : IState;

    internal static class Program
    {
        internal static void Main()
        {
            var initial = new MState(0);

            // Simple metrics collector instance
            var metrics = new SimpleMetrics();

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "INC") return new MState(((MState)state).Value + 1);
                return state;
            });

            // Pass metrics into the CreateStore overload that accepts a metrics implementation.
            var store = Factory.CreateStore(initial, reducer, metrics);

            store.Dispatch(Factory.CreateAction("INC"));
            Console.WriteLine($"DispatchCount: {metrics.DispatchCount}");
        }
    }

    // Very small example metrics implementation; production code should forward to a proper metrics system.
    internal sealed class SimpleMetrics : IStoreMetrics
    {
        // Exposed for example/readability; for real telemetry use long counters or specialized
        // metrics collectors that integrate with your telemetry backend.
        // Use `Interlocked` operations (or concurrent collectors) to avoid races when
        // instruments are updated from multiple threads.
        public int DispatchCount;

        // Called at the start of each dispatch. Increment is thread-safe via Interlocked.
        public void OnDispatchStart() => Interlocked.Increment(ref DispatchCount);

        // Called at dispatch end with elapsed duration — record into your histogram/aggregator.
        public void OnDispatchEnd(TimeSpan elapsed) { }

        // Called when middleware starts; include middleware names or ids in production usage.
        public void OnMiddlewareStart(string middlewareId) { }

        public void OnMiddlewareEnd(string middlewareId, TimeSpan elapsed) { }
    }
}
