
/*
 * Example 05 - Nested slices (advanced / not recommended)
 * Purpose:
 *   Illustrates a nested slice model and highlights why nested slices increase
 *   complexity compared to a flat slice map. Use this only when your domain
 *   model truly requires nested state — otherwise prefer flattening.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using SC.AtomicState;

namespace Examples
{
    public sealed record InnerState(int X) : IState;
    public sealed record OuterState(InnerState Inner) : IState;

    internal static class Program
    {
        internal static void Main()
        {

            // Example: nested slices by embedding one IState inside another.
            // NOTE: This pattern is possible but generally not recommended because it complicates change detection
            // and requires careful reference-preservation checks when composing reducers.
            // When using nested slices you must be explicit about preserving references
            // for unchanged nested objects to avoid unnecessary publications.

            // Inner reducer handles actions targeted at the inner portion of the state.
            var innerSlice = Factory.CreateSlice("inner", new InnerState(0), new Reducer<InnerState>((s, a) =>
            {
                if (a.Type == "INC_INNER")
                    return new InnerState(s.X + 1);
                return s;
            }));

            // Outer reducer must coordinate updates to the embedded InnerState. Because `Slice` internals are internal,
            // we implement the inner update inline here to keep the example self-contained.
            // The key idea: compute the candidate inner state first, then only allocate
            // a new outer instance if the inner actually changed (preserve identity otherwise).
            var outerSlice = Factory.CreateSlice("outer", new OuterState(new InnerState(0)), new Reducer<OuterState>((s, a) =>
            {
                if (a.Type == "INC_INNER")
                {
                    // Compute the next inner state (duplicate of inner reducer logic for demonstration)
                    var nextInner = a.Type == "INC_INNER" ? new InnerState(s.Inner.X + 1) : s.Inner;

                    // Preserve outer instance when inner didn't change.
                    if (ReferenceEquals(nextInner, s.Inner)) return s;
                    return new OuterState(nextInner);
                }

                return s;
            }));

            var (rootState, rootReducer) = Factory.CombineReducers(outerSlice);
            var store = Factory.CreateStore(rootState, rootReducer);

            Console.WriteLine("Nested slices example: use sparingly; flatten when possible for simpler reasoning and fewer allocations.");
        }
    }
}
