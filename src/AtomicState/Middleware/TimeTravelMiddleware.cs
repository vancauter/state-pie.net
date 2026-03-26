namespace SC.AtomicState.MiddlewareEx
{
    public static class TimeTravelMiddleware
    {
        public static Middleware Create(TimeTravelOptions options, Func<IState> getState, Func<IState, Task> applyStateAsync, out ITimeTravelController controller)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(getState);
            ArgumentNullException.ThrowIfNull(applyStateAsync);

            var ctl = new TimeTravelController(options, applyStateAsync);
            controller = ctl;

            // Initial snapshot will be recorded on first dispatch to avoid requiring the store instance
            // to be available at middleware construction time.

            return (ctx, next) => action =>
            {
                // Call next first to allow state to be updated by reducers.
                next(action);

                try
                {
                    // Record the committed state after dispatch completes.
                    var s = getState();
                    // Optionally avoid recording time-travel internal actions here; simplified: record all.
                    ctl.Record(s, action);
                }
                catch
                {
                    // Swallow to avoid impacting dispatch semantics.
                }
            };
        }
    }

    public record TimeTravelOptions(int MaxHistory = 100, bool StoreActions = true, bool StoreSnapshots = false, bool UseDiffs = true);
}
