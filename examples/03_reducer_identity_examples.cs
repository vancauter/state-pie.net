/*
 * Example 03 - Reducer identity-preservation
 * Purpose:
 *   Demonstrates correct reducer patterns: preserve the same reference when the
 *   state is unchanged, and avoid the anti-pattern of always allocating new
 *   instances for no-op actions. This is important for performance and for
 *   subscribers that rely on reference-equality checks.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using SC.AtomicState;

namespace Examples
{
    public sealed record RState(int Value) : IState;

    internal static class Program
    {
        internal static void Main()
        {
            // GOOD: preserve the existing instance for no-op actions.
            // Why: Returning the same reference avoids publishing state changes to
            // observers when nothing actually changed, and it reduces allocations.
            var reducerGood = new Reducer((state, action) =>
            {
                if (action.Type == "NOOP")
                    return state; // Good: preserve reference

                if (action.Type == "SET" && action is IAction<int> p)
                    return new RState(p.Payload);

                return state;
            });

            // BAD: always allocating a new instance even when values are unchanged.
            // Consequences:
            // - Every dispatch creates a fresh object, increasing GC pressure.
            // - Observers will see a different reference even when semantically identical
            //   which can trigger expensive downstream computations.
            var reducerBad = new Reducer((state, action) =>
            {
                if (action.Type == "NOOP")
                    return new RState(((RState)state).Value); // BAD: returns a new object unnecessarily

                return state;
            });

            Console.WriteLine("Reducer identity guidance: prefer returning the existing instance when unchanged.");

            // Use so IDE doesn't warn about unused variables
            // You can remove these in real code
            Console.WriteLine(reducerGood);
            Console.WriteLine(reducerBad);
        }
    }
}
