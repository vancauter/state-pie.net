/*
 * Example 06 - Persistence
 * Purpose:
 *   Demonstrates `PersistMiddleware` and an in-memory `ISnapshotStore` test double.
 *   Shows how middleware can be used to persist snapshots transparently after
 *   successful dispatches and how to explicitly save/load snapshots in tests.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using System.Threading.Tasks;
using SC.AtomicState;
using SC.AtomicState.Persistence;

namespace Examples
{
    public sealed record PersistState(string Name, int Counter) : IState;

    internal static class Program
    {
        internal static async Task Main()
        {
            var initial = new PersistState("none", 0);

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "SET_NAME" && action is IAction<string> pa)
                    return new PersistState(pa.Payload, ((PersistState)state).Counter);
                if (action.Type == "INC")
                    return new PersistState(((PersistState)state).Name, ((PersistState)state).Counter + 1);
                return state;
            });

            // In-memory test double for ISnapshotStore (fast and deterministic)
            var inMem = new InMemorySnapshotStore();

            // PersistMiddleware requires a delegate that returns the current root state.
            IStore store = null;
            var middleware = PersistMiddleware.Create(inMem, "examples/persist", () => store.State);
            // PersistMiddleware will save snapshots after each successful dispatch. The middleware is opt-in.
            // Note: use a durable `ISnapshotStore` in production; this in-memory store is useful
            // for fast examples and unit tests where deterministic behavior is important.
            store = Factory.CreateStore(initial, reducer, middleware);

            // Perform some updates
            store.Dispatch(Factory.CreateAction("SET_NAME", "alice"));
            store.Dispatch(Factory.CreateAction("INC"));

            // Save and load via the snapshot store explicitly (the middleware also saves automatically)
            // This demonstrates a persistence round-trip without touching disk and mirrors
            // typical test assertions that verify snapshot fidelity after dispatches.
            await inMem.SaveAsync("examples/persist", store.State);
            var loaded = await inMem.LoadAsync("examples/persist");
            Console.WriteLine($"Loaded name: {((PersistState)loaded).Name}, counter: {((PersistState)loaded).Counter}");
        }
    }
}
