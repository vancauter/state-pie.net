/*
 * Example 08 - Time travel (undo/redo)
 * Purpose:
 *   Shows how `TimeTravelMiddleware` records snapshots and exposes a controller
 *   that can apply undo/redo operations. Demonstrates safe store snapshot seeding
 *   and application patterns used by the middleware.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using System.Threading.Tasks;
using SC.AtomicState;
using SC.AtomicState.MiddlewareEx;
using SC.AtomicState.Persistence;

namespace Examples
{
    public sealed record TTState(int Value) : IState;

    internal static class Program
    {
        internal static async Task Main()
        {
            var initial = new TTState(0);

            // The store instance is created after the middleware so we pass a 'getState' delegate that captures the
            // store variable. This avoids circular construction issues where middleware needs to read the current
            // state but the store has not yet been assigned.
            // Use the null-forgiving pattern when capturing the variable before assignment in nullable-enabled projects.
            IStore store = null;

            var middleware = TimeTravelMiddleware.Create(
                new TimeTravelOptions(),
                () => store.State,
                s => StoreSnapshotHelper.SeedStoreAsync(store, s),
                out var controller);

            var reducer = new Reducer((state, action) =>
            {
                if (action.Type == "INC")
                    return new TTState(((TTState)state).Value + 1);
                return state;
            });

            store = Factory.CreateStore(initial, reducer, middleware);

            // Make a couple of commits
            store.Dispatch(Factory.CreateAction("INC"));
            store.Dispatch(Factory.CreateAction("INC"));

            Console.WriteLine($"Value before undo: {((TTState)store.State).Value}");
            // Undo the last commit. The controller uses the provided apply delegate to
            // seed the store with the previous snapshot — this mimics how an undo
            // operation can be implemented without exposing internal snapshot details.
            await controller.UndoAsync();
            Console.WriteLine($"Value after undo: {((TTState)store.State).Value}");
        }
    }
}
