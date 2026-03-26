/*
 * Example 04 - Flat slices (recommended)
 * Purpose:
 *   Show how to create multiple small slice reducers, each focused on a single
 *   piece of the root state, and then combine them. This approach simplifies
 *   reducers and makes change detection and identity-preservation easier.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using SC.AtomicState;

namespace Examples
{
    public sealed record NameState(string Name) : IState;
    public sealed record CountState(int Count) : IState;

    internal static class Program
    {
        internal static void Main()
        {

            // Create a typed slice handle for the `name` slice.
            // Rationale:
            // - Each slice owns its portion of state and its reducer logic.
            // - Typed slice reducers improve correctness by making the slice state
            //   explicit and avoid repeated casts.
            var nameSlice = Factory.CreateSlice("name", new NameState("x"), new Reducer<NameState>((s, a) =>
            {
                if (a is IAction<string> pa && pa.Type == "SET_NAME")
                    return new NameState(pa.Payload);
                return s;
            }));

            // Create a typed slice handle for the `count` slice.
            var countSlice = Factory.CreateSlice("count", new CountState(0), new Reducer<CountState>((s, a) =>
            {
                if (a.Type == "INC")
                    return new CountState(s.Count + 1);
                return s;
            }));

            // Combine the slice handles into a root state and reducer pair. The
            // combined reducer delegates to each slice reducer and composes a
            // predictable root state shape that callers can inspect via `State`.
            var (rootState, rootReducer) = Factory.CombineReducers(nameSlice, countSlice);

            // Create the store using the combined reducer pair. The store will
            // maintain the composed root state and publish updates when slices change.
            var store = Factory.CreateStore(rootState, rootReducer);

            // Dispatch actions that target individual slices. The combined reducer delegates to each slice reducer.
            store.Dispatch(Factory.CreateAction("SET_NAME", "bob"));
            store.Dispatch(Factory.CreateAction("INC"));

            Console.WriteLine("Flat slices example: prefer a flat map of slice states for simpler reducers and change detection.");
        }
    }
}
