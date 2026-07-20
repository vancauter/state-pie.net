/*
 * Example 01 - Basic usage
 * Purpose:
 *   Demonstrates the minimal, recommended pattern for creating an AtomicState store:
 *   define an immutable root state, create a reducer that preserves identity when
 *   nothing changes, create the store, and dispatch actions.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using SC.AtomicState;

namespace Examples
{
    public sealed record BasicState(int Count) : IState;

    internal static class Program
    {
        internal static void Main()
        {
            // Initial root state snapshot for the store
            var initial = new BasicState(0);

            // Reducer: a pure function that returns a new state instance only when something changes.
            // Important guidance:
            // - If the action does not change the state, return the same `state` instance
            //   to preserve identity. Many consumers and optimizations rely on reference
            //   equality to detect changes cheaply and avoid unnecessary allocations.
            // - Only create a new instance when the logical state actually differs.
            var reducer = new Reducer((state, action) =>
            {
                // Simple action handling by string-based action type
                if (action.Type == "INC")
                    return new BasicState(((BasicState)state).Count + 1);

                // No change — return the same instance (do not allocate an identical copy)
                return state;
            });

            // Create the store with the initial state and reducer. The store immediately
            // computes the initial committed state by feeding a synthetic init action
            // through the reducer — this allows reducers to compute derived values.
            var store = Factory.CreateStore(initial, reducer);

            // Dispatch a simple action. Dispatch is synchronous: observers subscribed
            // to the store will receive notifications as part of the dispatch pipeline.
            store.Dispatch(Factory.CreateAction("INC"));

            // Read the committed state after dispatch. Casting back to the concrete
            // record type is necessary because `IState` is the untyped root interface.
            Console.WriteLine($"Count: {((BasicState)store.State).Count}");
        }
    }
}
