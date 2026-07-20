/*
 * Example 07 - Middleware and background work
 * Purpose:
 *   Demonstrates the recommended pattern for scheduling background work from
 *   middleware and safely dispatching from that background context using
 *   `SafeDispatchAsync`. Middleware should avoid blocking the dispatch pipeline.
 * Author: S. van Cauter
 * Last updated: 2026-03-26
 */

using System;
using System.Threading.Tasks;
using SC.AtomicState;
using SC.AtomicState.Helpers;

namespace Examples
{
    public sealed record MwState(int Value) : IState;

    internal static class Program
    {
        internal static void Main()
        {
            var initial = new MwState(0);

            // Middleware that logs and schedules background work which later dispatches another action.
            Dispatch loggingMiddleware(IDispatchContext ctx, Dispatch next) => action =>
            {
                // Synchronous logging on the dispatch stack (fast and safe)
                Console.WriteLine($"Before action: {action.Type}");

                // Schedule background work so we don't await on the dispatch thread.
                // `ScheduleBackground` ensures cooperative yields and runs the provided delegate on the thread-pool.
                MiddlewareHelpers.ScheduleBackground(async () =>
                {
                    // Simulate asynchronous work
                    await Task.Delay(10);

                    // Dispatch from background work using helper that avoids blocking the dispatch pipeline.
                    await MiddlewareHelpers.SafeDispatchAsync(ctx, Factory.CreateAction("INC_FROM_BG"));
                });

                // Continue the synchronous dispatch pipeline.
                next(action);
            };

            // Reducer handles both direct and background-driven increments.
            var reducer = new Reducer((state, action) =>
            {
                return action.Type switch
                {
                    "INC" => new MwState(((MwState)state).Value + 1),
                    "INC_FROM_BG" => new MwState(((MwState)state).Value + 1),
                    _ => state
                };
            });

            var store = Factory.CreateStore(initial, reducer, loggingMiddleware);

            store.Dispatch(Factory.CreateAction("INC"));
            Console.WriteLine($"Value: {((MwState)store.State).Value}");
        }
    }
}
