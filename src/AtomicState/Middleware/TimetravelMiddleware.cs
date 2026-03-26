using SC.AtomicState.Persistence;

namespace SC.AtomicState.MiddlewareEx
{
    public static class TimetravelMiddleware
    {
        public static global::SC.AtomicState.Middleware Create(TimeTravelOptions options, out ITimetravelController controller)
        {
            ArgumentNullException.ThrowIfNull(options);

            var ctl = new TimetravelController(async state => { /* apply is performed by middleware when needed */ await Task.CompletedTask; });
            controller = ctl;

            return (ctx, next) => action =>
            {
                // Call next first to allow state to be updated by reducers.
                next(action);

                // After successful dispatch, capture snapshot via ctx.DispatchContext? We only have access to ctx and next.
                // Middleware cannot directly read the committed state; rely on Store exposing state via a captured delegate is better.
                // For this simplified implementation, assume middleware author captures a getState delegate via closure in a higher-level factory.
            };
        }
    }

    public record TimeTravelOptions(int MaxHistory = 100, bool StoreActions = true, bool StoreSnapshots = false, bool UseDiffs = true);
}
