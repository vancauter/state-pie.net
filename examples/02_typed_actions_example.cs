/*
 * Example 02 - Typed actions
 * Purpose:
 *   Showcases `TypedActionId<T>` as a single source of truth for action identity
 *   and typed payloads. Avoids spreading magic strings and makes reducers safer.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using SC.AtomicState;
using SC.AtomicState.Helpers;

namespace Examples
{
    public sealed record PersonState(string Name) : IState;

    internal static class Program
    {
        internal static void Main()
        {

            // Define a typed action id once and reuse it throughout the codebase.
            // Rationale:
            // - The `TypedActionId<T>` binds a payload type `T` to a stable action string.
            // - This avoids scattering magic strings and enables safer reducers and callers.
            var setName = new TypedActionId<string>("@Person/SetName");

            var initial = new PersonState("anon");

            var reducer = new Reducer((state, action) =>
            {
                // Pattern-match for the strongly-typed action and compare by Type.
                // Note: using the typed action id's `Type` string keeps a single
                // canonical action name while still exposing a typed payload.
                if (action is IAction<string> pa && pa.Type == setName.Type)
                {
                    // Use the typed payload safely — no downstream casts required.
                    return new PersonState(pa.Payload);
                }

                return state;
            });

            var store = Factory.CreateStore(initial, reducer);

            // Create an action using the typed identifier and a payload value.
            // The `Factory.CreateAction` overload preserves the typed payload at runtime.
            store.Dispatch(Factory.CreateAction(setName, "Alice"));

            // Observe the committed state after dispatch. Casting is required because
            // store.State is the untyped `IState` root; convert to your concrete type.
            Console.WriteLine($"Name: {((PersonState)store.State).Name}");
        }
    }
}
